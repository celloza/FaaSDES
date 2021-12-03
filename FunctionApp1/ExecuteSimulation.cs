using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FaaSDES.Sim;
using System.Xml.Linq;
using FaaSDES.Sim.Tokens.Generation;

namespace FunctionApp1
{
    public static class FaasDESSimulation
    {
        [FunctionName("ExecuteSimulation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation($"Received BPMN XML configuration:");
            log.LogInformation(requestBody);

            Simulator simulator = Simulator.FromBpmnXML(requestBody);

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

            log.LogInformation($"Execution completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds):G}");


            string responseMessage = "Simulation complete.";


            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
