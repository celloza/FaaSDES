using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FaaSDES.Sim;
using FaaSDES.Sim.NodeStatistics;
using FaaSDES.Sim.Tokens.Generation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using FaaSDES.Functions.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace FaaSDES.Functions
{
    public static class SimulationOrchestrator
    {
        [FunctionName("SimulationOrchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var data = context.GetInput<SimulationRequest>();

            log.LogInformation($"Received BPMN XML configuration:");
            log.LogInformation(data.GetBpmnXmlData());

            var parallelTasks = new List<Task<string>>();

            for (int i = 0; i < data.Iterations; i++)
            {
                log.LogInformation(@"Creating task #" + i);
                Task<string> task = context.CallActivityAsync<string>("SimulationOrchestrator_ExecuteSim", data);
                parallelTasks.Add(task);
            }

            await Task.WhenAll(parallelTasks);

            foreach(var task in parallelTasks)
            {
                Task<string> flushTask = context.CallActivityAsync<string>("SimulationOrchestrator_FlushLogs",
                    System.Text.Json.JsonSerializer.Deserialize<List<EventStatistic>>(task.Result));
            }

            // Aggregate all N outputs and send the result to F3.
            //string result = string.Join("\n\r", parallelTasks);
            //await context.CallActivityAsync("F3", sum);

            return parallelTasks.Select(x => x.Result).ToList();
        }

        [FunctionName("SimulationOrchestrator_ExecuteSim")]
        public static string ExecuteSim([ActivityTrigger] SimulationRequest xmlContent, ILogger log) //, IAsyncCollector<EventStatistic> outputTable
        {
            log.LogInformation("Execute sim activity started.");

            Simulator simulator = Simulator.FromBpmnXML(xmlContent.GetBpmnXmlData());

            var tokenGenerator = new TimerSimTokenGenerator(
                            new GenerationSettings(0, 5),
                            new TimeOnly(8, 0),
                            new TimeOnly(15, 0),
                            new WeekDaySchedule(true, true, true, true, true, false, false));

            log.LogInformation(tokenGenerator.ToString());

            var simSettings = new SimulationSettings()
            {
                StartDateTime = new DateTime(2021, 01, 01),
                EndDateTime = new DateTime(2023, 12, 31),
                MaximumIterations = int.MaxValue,
                TimeFactor = new TimeSpan(0, 1, 0),
                TokenMaxQueueTime = new TimeSpan(1, 30, 0)
            };

            log.LogInformation(simSettings.ToString());

            var simulation = simulator.NewSimulationInstance(simSettings, tokenGenerator);

            log.LogInformation(simulation.ToString());
            log.LogInformation("Starting simulation execution...");

            System.Diagnostics.Stopwatch sw = new();
            sw.Start();

            simulation.Execute();

            sw.Stop();

            log.LogInformation($"Execution of simulation completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds):G}");

            log.LogInformation("Dumping results...");

            return System.Text.Json.JsonSerializer.Serialize(simulation.GetAllEventStatistics().ToList());
        }

        [FunctionName("SimulationOrchestrator_FlushLogs")]
        public static async Task<string> FlushLogs([ActivityTrigger] List<EventStatistic> statistics, 
            ILogger log)
            
        {
            log.LogInformation("Flush logs activity started.");

            foreach(var stat in statistics)
            {
                //await statTableCollector.AddAsync(stat.ToTableEntity());

            }

            return "Logs queued for flushing.";
        }

        [FunctionName("SimulationOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var data = req.Content.ReadAsStringAsync();
            SimulationRequest request = System.Text.Json.JsonSerializer.Deserialize<SimulationRequest>(data.Result);

            //Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("SimulationOrchestrator", request);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
            //return null;
        }
    }
}