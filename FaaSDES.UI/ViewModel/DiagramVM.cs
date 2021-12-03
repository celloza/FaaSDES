using Microsoft.Win32;
using FaaSDES.UI.Utility;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Diagram.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using Syncfusion.XPS;
using Syncfusion.Pdf;
using FaaSDES.UI.Nodes;
using System.Diagnostics;
using FaaSDES.Sim.Tokens.Generation;

namespace FaaSDES.UI.ViewModel
{
    public class DiagramVM : DiagramViewModel
    {
        #region Fields

        private ICommand _NewCommand;
        private ICommand _SaveCommand;
        private ICommand _LoadCommand;
        private ICommand _MenuOpenCommand;
        private ICommand _MenuItemClickedCommand;
        private ICommand _SelectionChangedCommand;

        #endregion
        #region private variables

        private string _SavedPath;
        private bool selectToolProperty = true;
        private bool panToolProperty = false;
        private bool drawToolProperty = false;
        private bool isItemSelected = false;
        private string zoomPercentageValue = "30%";
        private bool straightProperty = false;
        private bool orthogonalProperty = true;
        private bool bezierProperty = false;
        private bool landscapeProperty = true;
        private bool portraitProperty = false;
        private bool _ZoomInEnabled = true;
        private bool _ZoomOutEnabled = true;
        private double _currentZoom = 1;
        private FaaSDESNodeViewModel _selectedFaaSDESActivityNodeViewModel = null;

        #endregion
        public DiagramVM()
        {
            this.Nodes = new ObservableCollection<NodeViewModel>();
            this.Connectors = new ObservableCollection<ConnectorViewModel>();
            this.Groups = new ObservableCollection<GroupViewModel>();
            this.PageSettings = new PageSettings()
            {
                PageWidth = 1500,
                PageHeight = 816,
                ShowPageBreaks = true,
                MultiplePage = true,
            };
            this.HorizontalRuler = new Ruler();
            this.VerticalRuler = new Ruler() { Orientation = Orientation.Vertical };
            this.PrintingService = new PrintingService();
            this.Constraints |= GraphConstraints.Undoable | GraphConstraints.AllowPan;
            this.SelectedItems = new SelectorViewModel()
            {
            };
            (this.SelectedItems as SelectorViewModel).SelectorConstraints &= ~SelectorConstraints.QuickCommands;
            this.PortVisibility = PortVisibility.Collapse;
            this.ExportSettings = new ExportSettings()
            {
            };
            this.SnapSettings = new SnapSettings()
            {
                SnapConstraints = SnapConstraints.ShowLines | SnapConstraints.SnapToLines,
                SnapToObject = SnapToObject.All,
            };
            this.ScrollSettings = new ScrollSettings()
            {
            };

            InitializeDiagram();
            NewCommand = new Command(OnNew);
            LoadCommand = new Command(OnLoad);
            SaveCommand = new Command(OnSave);
            MenuOpenCommand = new Command(OnMenuOpenCommand);
            MenuItemClickedCommand = new Command(OnMenuItemClickedCommand);
            PrintCommand = new Command(this.OnPrintCommand);
            ExportCommand = new Command(this.OnExportCommand);
            SelectToolCommand = new Command(this.OnSelectToolCommand);
            PanToolCommand = new Command(this.OnPanToolCommand);
            RotateColockwiseCommand = new Command(OnRotateColockwiseCommand);
            RotateCounterColockwiseCommand = new Command(OnRotateCounterColockwiseCommand);
            SelectNoneCommand = new Command(OnSelectNoneCommand);
            ZoomInCommand = new Command(OnZoomInCommand);
            ZoomOutCommand = new Command(OnZoomOutCommand);
            SnapToGridCommand = new Command(OnSnapToGridCommand);
            ShowLinesCommand = new Command(OnShowLinesCommand);
            SnapToObjectCommand = new Command(OnSnapToObjectCommand);
            ShowRulerCommand = new Command(OnShowRulerCommand);
            ItemSelectedCommand = new Command(OnItemSelectedCommand);
            ItemUnSelectedCommand = new Command(OnItemUnSelectedCommand);
            FitToWidthCommand = new Command(OnFitToWidthCommand);
            FitToPageCommand = new Command(OnFitToPageCommand);
            ViewPortChangedCommand = new Command(OnViewPortChangedCommand);
            SelectAllNodesCommand = new Command(OnSelectAllNodesCommand);
            SelectAllConnectorsCommand = new Command(OnSelectAllConnectorsCommand);
            LoadBlankDiagramCommand = new Command(OnLoadBlankDiagramCommand);
            ChangeConnectorTypeCommand = new Command(OnChangeConnectorTypeCommand);
            DrawConnectorCommand = new Command(OnDrawConnectorCommand);
            OrientationCommand = new Command(OnOrientationCommand);
            ShowPageBreaksCommand = new Command(OnShowPageBreaksCommand);
            ButtonMenuOpeningCommand = new Command(OnButtonMenuOpeningCommand);
            ItemAddedCommand = new Command(OnItemAddedCommandCommand);
            SelectionChangedCommand = new Command(OnSelectionChangedCommand);
        }

        #region Commands
        public ICommand PrintCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand SelectToolCommand { get; set; }
        public ICommand PanToolCommand { get; set; }
        public ICommand DrawConnectorCommand { get; set; }
        public ICommand RotateColockwiseCommand { get; set; }
        public ICommand RotateCounterColockwiseCommand { get; set; }
        public ICommand SelectNoneCommand { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand SnapToGridCommand { get; set; }
        public ICommand ShowLinesCommand { get; set; }
        public ICommand SnapToObjectCommand { get; set; }
        public ICommand SelectAllNodesCommand { get; set; }
        public ICommand SelectAllConnectorsCommand { get; set; }
        public ICommand LoadBlankDiagramCommand { get; set; }
        public ICommand LoadDefaultNetworkDiagramCommand { get; set; }
        public ICommand ChangeConnectorTypeCommand { get; set; }
        public ICommand OrientationCommand { get; set; }
        public ICommand ShowRulerCommand { get; set; }
        public ICommand ShowPageBreaksCommand { get; set; }
        public ICommand FitToWidthCommand { get; set; }
        public ICommand FitToPageCommand { get; set; }
        public ICommand ButtonMenuOpeningCommand { get; set; }
        public ICommand ItemAddedCommand { get; set; }

        public ICommand MenuItemClickedCommand
        {
            get { return _MenuItemClickedCommand; }
            set { _MenuItemClickedCommand = value; }
        }

        public ICommand NewCommand
        {
            get { return _NewCommand; }
            set { _NewCommand = value; }
        }

        public ICommand LoadCommand
        {
            get { return _LoadCommand; }
            set { _LoadCommand = value; }
        }

        public ICommand SaveCommand
        {
            get { return _SaveCommand; }
            set { _SaveCommand = value; }
        }

        public ICommand MenuOpenCommand
        {
            get { return _MenuOpenCommand; }
            set { _MenuOpenCommand = value; }
        }

        public ICommand SelectionChangedCommand
        {
            get { return _SelectionChangedCommand; }
            set { _SelectionChangedCommand = value; }
        }

        #endregion
        #region public properties

        public FaaSDESNodeViewModel SelectedFaaSDESActivityNodeViewModel
        {
            get { return _selectedFaaSDESActivityNodeViewModel; }
            set
            {
                _selectedFaaSDESActivityNodeViewModel = value;
                OnPropertyChanged("SelectedFaaSDESActivityNodeViewModel");
            }
        }

        public bool SelectToolProperty
        {
            get
            {
                return selectToolProperty;
            }
            set
            {
                if (selectToolProperty != value)
                {
                    selectToolProperty = value;
                    this.OnSelectToolCommand(!value);
                    OnPropertyChanged("SelectToolProperty");
                }
            }
        }

        public string ZoomPercentageValue
        {
            get
            {
                return zoomPercentageValue;
            }
            set
            {
                if (zoomPercentageValue != value)
                {
                    zoomPercentageValue = value + "%";
                    OnPropertyChanged("ZoomPercentageValue");
                }
            }
        }

        public bool PanToolProperty
        {
            get
            {
                return panToolProperty;
            }
            set
            {
                if (panToolProperty != value)
                {
                    panToolProperty = value;
                    this.OnPanToolCommand(!value);
                    OnPropertyChanged("PanToolProperty");
                }
            }
        }

        public bool StraightProperty
        {
            get
            {
                return straightProperty;
            }
            set
            {
                if (straightProperty != value)
                {
                    straightProperty = value;
                    OnPropertyChanged("StraightProperty");
                }
            }
        }

        public bool OrthogonalProperty
        {
            get
            {
                return orthogonalProperty;
            }
            set
            {
                if (orthogonalProperty != value)
                {
                    orthogonalProperty = value;
                    OnPropertyChanged("OrthogonalProperty");
                }
            }
        }

        public bool BezierProperty
        {
            get
            {
                return bezierProperty;
            }
            set
            {
                if (bezierProperty != value)
                {
                    bezierProperty = value;
                    OnPropertyChanged("BezierProperty");
                }
            }
        }

        public bool DrawToolProperty
        {
            get
            {
                return drawToolProperty;
            }
            set
            {
                if (drawToolProperty != value)
                {
                    drawToolProperty = value;
                    this.OnDrawConnectorCommand(value);
                    OnPropertyChanged("DrawToolProperty");
                }
            }
        }

        public bool IsSelectToolSelected
        {
            get
            {
                return selectToolProperty;
            }
            set
            {
                if (selectToolProperty != value)
                {
                    selectToolProperty = value;
                    this.OnSelectToolCommand(!value);
                    OnPropertyChanged("IsSelectToolSelected");
                }
            }
        }

        public bool LandscapeProperty
        {
            get
            {
                return landscapeProperty;
            }
            set
            {
                if (landscapeProperty != value)
                {
                    landscapeProperty = value;
                    OnPropertyChanged("LandscapeProperty");
                }
            }
        }

        public bool PortraitProperty
        {
            get
            {
                return portraitProperty;
            }
            set
            {
                if (portraitProperty != value)
                {
                    portraitProperty = value;
                    OnPropertyChanged("PortraitProperty");
                }
            }
        }

        public bool IsPanToolSelected
        {
            get
            {
                return panToolProperty;
            }
            set
            {
                if (panToolProperty != value)
                {
                    panToolProperty = value;
                    this.OnPanToolCommand(!value);
                    OnPropertyChanged("IsPanToolSelected");
                }
            }
        }

        public bool IsItemSelected
        {
            get
            {
                return isItemSelected;
            }
            set
            {
                if (isItemSelected != value)
                {
                    isItemSelected = value;
                    OnPropertyChanged("IsItemSelected");
                }
            }
        }

        public bool ZoomInEnabled
        {
            get
            {
                return _ZoomInEnabled;
            }
            set
            {
                if (value != _ZoomInEnabled)
                {
                    _ZoomInEnabled = value;
                    OnPropertyChanged("ZoomInEnabled");
                }
            }
        }

        public bool ZoomOutEnabled
        {
            get
            {
                return _ZoomOutEnabled;
            }
            set
            {
                if (value != _ZoomOutEnabled)
                {
                    _ZoomOutEnabled = value;
                    OnPropertyChanged("ZoomOutEnabled");
                }
            }
        }

        public double CurrentZoom
        {
            get
            {
                return _currentZoom;
            }
            set
            {
                if (value != _currentZoom)
                {
                    _currentZoom = value;
                    (this.Info as IGraphInfo).Commands.Zoom.Execute(new ZoomPositionParameter()
                    {
                        ZoomCommand = ZoomCommand.Zoom,
                        ZoomTo = _currentZoom,
                    });
                    OnPropertyChanged("CurrentZoom");
                }
            }
        }
        #endregion
        #region Helper Methods
        /// <summary>
        /// This method is used to execute Save command
        /// </summary>
        /// <param name="obj"></param>
        private void OnMenuOpenCommand(object obj)
        {
            BpmnNode bpmnnode = (obj as MenuOpeningEventArgs).Source as BpmnNode;
            BpmnGroup bpmngroup = (obj as MenuOpeningEventArgs).Source as BpmnGroup;
            BpmnFlow bpmnflow = (obj as MenuOpeningEventArgs).Source as BpmnFlow;
            if (bpmnnode != null)
            {
                var node = bpmnnode.DataContext as BpmnNodeViewModel;
                if (node != null)
                {
                    if (node.Type == BpmnShapeType.TextAnnotation)
                    {
                        AddAnnotationMenuItems(node);
                    }
                    else if (node.Type == BpmnShapeType.Activity)
                    {
                        AddActivityMenuItems(node);
                    }
                    else if (node.Type == BpmnShapeType.Gateway)
                    {
                        AddGatewayMenuItems(node);
                    }
                    else if (node.Type == BpmnShapeType.Event)
                    {
                        AddEventMenuItems(node);
                    }
                    else if (node.Type == BpmnShapeType.DataObject)
                    {
                        AddDataObjectMenuItems(node);
                    }
                }
            }
            if (bpmngroup != null)
            {
                var node = bpmngroup.DataContext as BpmnGroupViewModel;
                if (node.IsExpandedSubProcess)
                {
                    AddExpandedSubProcessMenuItems(node);
                }
            }
            if (bpmnflow != null)
            {
                var flow = bpmnflow.DataContext as BpmnFlowViewModel;
                AddBpmnFlowMenuItems(flow);
            }
        }

        private void OnMenuItemClickedCommand(object obj)
        {
            var menuitem = obj as MenuItemClickedEventArgs;
            var bpmnnode = menuitem.Source as BpmnNode;
            var bpmngroup = menuitem.Source as BpmnGroup;
            var bpmnflow = menuitem.Source as BpmnFlow;
            if (bpmnnode != null)
            {
                var node = bpmnnode.DataContext as BpmnNodeViewModel;
                if (node != null)
                {
                    if (node.Type == BpmnShapeType.TextAnnotation)
                    {
                        UpdateAnnotationShapeValue(node, menuitem);
                    }
                    else if (node.Type == BpmnShapeType.Activity)
                    {
                        UpdateActivityShapeValue(node, menuitem);
                    }
                    else if (node.Type == BpmnShapeType.Gateway)
                    {
                        UpdateGatewayShapeValue(node, menuitem);
                    }
                    else if (node.Type == BpmnShapeType.Event)
                    {
                        UpdateEventShapeValue(node, menuitem);
                    }
                    else if (node.Type == BpmnShapeType.DataObject)
                    {
                        UpdateDataObjectShapeValue(node, menuitem);
                    }
                }
            }
            if (bpmngroup != null)
            {
                var node = bpmngroup.DataContext as BpmnGroupViewModel;
                if (node.IsExpandedSubProcess)
                {
                    UpdateExpandedSubProcessShapeValue(node, menuitem);
                }
            }
            if (bpmnflow != null)
            {
                var flow = bpmnflow.DataContext as BpmnFlowViewModel;
                UpdateBpmnFlowShapeValue(flow, menuitem);
            }
        }
        #region BpmnFlow
        private void AddBpmnFlowMenuItems(BpmnFlowViewModel flow)
        {
            flow.Constraints = flow.Constraints | ConnectorConstraints.Menu;
            flow.Menu = new DiagramMenu();
            flow.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "FlowType",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem m1 = new DiagramMenuItem()
            {
                Content = "Association",
                IsCheckable = flow.FlowType == BpmnFlowType.Association ? true : false
            };
            DiagramMenuItem m2 = new DiagramMenuItem()
            {
                Content = "BidirectedAssociation",
                IsCheckable = flow.FlowType == BpmnFlowType.BiDirectionalAssociation ? true : false
            };
            DiagramMenuItem m3 = new DiagramMenuItem()
            {
                Content = "ConditionalSequenceFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.ConditionalSequenceFlow ? true : false
            };
            DiagramMenuItem m4 = new DiagramMenuItem()
            {
                Content = "DefaultSequenceFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.DefaultSequenceFlow ? true : false
            };
            DiagramMenuItem m5 = new DiagramMenuItem()
            {
                Content = "DirectedAssociation",
                IsCheckable = flow.FlowType == BpmnFlowType.DirectionalAssociation ? true : false
            };
            DiagramMenuItem m6 = new DiagramMenuItem()
            {
                Content = "InitiatingMessageFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.InitiatingMessageFlow ? true : false
            };
            DiagramMenuItem m7 = new DiagramMenuItem()
            {
                Content = "MessageFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.MessageFlow ? true : false
            };
            DiagramMenuItem m8 = new DiagramMenuItem()
            {
                Content = "NonInitiatingMessageFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.NonInitiatingMessageFlow ? true : false
            };
            DiagramMenuItem m9 = new DiagramMenuItem()
            {
                Content = "SequenceFlow",
                IsCheckable = flow.FlowType == BpmnFlowType.SequenceFlow ? true : false
            };

            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m4);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m5);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m6);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m7);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m8);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m9);
            (flow.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
        }
        private void UpdateBpmnFlowShapeValue(BpmnFlowViewModel flow, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in flow.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                {
                    item.IsCheckable = false;
                }
            }
            (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Association")
            {
                flow.FlowType = BpmnFlowType.Association;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "DefaultSequenceFlow")
            {
                flow.FlowType = BpmnFlowType.DefaultSequenceFlow;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "BidirectedAssociation")
            {
                flow.FlowType = BpmnFlowType.BiDirectionalAssociation;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ConditionalSequenceFlow")
            {
                flow.FlowType = BpmnFlowType.ConditionalSequenceFlow;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "DirectedAssociation")
            {
                flow.FlowType = BpmnFlowType.DirectionalAssociation;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "InitiatingMessageFlow")
            {
                flow.FlowType = BpmnFlowType.InitiatingMessageFlow;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "MessageFlow")
            {
                flow.FlowType = BpmnFlowType.MessageFlow;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "NonInitiatingMessageFlow")
            {
                flow.FlowType = BpmnFlowType.NonInitiatingMessageFlow;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "SequenceFlow")
            {
                flow.FlowType = BpmnFlowType.SequenceFlow;
            }
        }
        #endregion

        #region AnnotationShape
        private void AddAnnotationMenuItems(BpmnNodeViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            //node.Constraints = node.Constraints & ~NodeConstraints.InheritMenu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "CalloutDirection",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem m1 = new DiagramMenuItem()
            {
                Content = "Left",
                IsCheckable = node.TextAnnotationDirection == TextAnnotationDirection.Left ? true : false
            };
            DiagramMenuItem m2 = new DiagramMenuItem()
            {
                Content = "Right",
                IsCheckable = node.TextAnnotationDirection == TextAnnotationDirection.Right ? true : false
            };
            DiagramMenuItem m3 = new DiagramMenuItem()
            {
                Content = "Top",
                IsCheckable = node.TextAnnotationDirection == TextAnnotationDirection.Top ? true : false
            };
            DiagramMenuItem m4 = new DiagramMenuItem()
            {
                Content = "Bottom",
                IsCheckable = node.TextAnnotationDirection == TextAnnotationDirection.Bottom ? true : false
            };
            DiagramMenuItem m5 = new DiagramMenuItem()
            {
                Content = "Auto",
                IsCheckable = node.TextAnnotationDirection == TextAnnotationDirection.Auto ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m4);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m5);
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
        }
        private void UpdateAnnotationShapeValue(BpmnNodeViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                {
                    item.IsCheckable = false;
                }
            }
            (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Left")
            {
                node.TextAnnotationDirection = TextAnnotationDirection.Left;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Top")
            {
                node.TextAnnotationDirection = TextAnnotationDirection.Top;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Right")
            {
                node.TextAnnotationDirection = TextAnnotationDirection.Right;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Bottom")
            {
                node.TextAnnotationDirection = TextAnnotationDirection.Bottom;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Auto")
            {
                node.TextAnnotationDirection = TextAnnotationDirection.Auto;
            }
        }
        #endregion

        #region Activity

        private void AddActivityMenuItems(BpmnNodeViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "Loop",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem l1 = new DiagramMenuItem()
            {
                Content = "None",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.None ? true : false
            };
            DiagramMenuItem l2 = new DiagramMenuItem()
            {
                Content = "Standard",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.Standard ? true : false
            };
            DiagramMenuItem l3 = new DiagramMenuItem()
            {
                Content = "ParallelMultiInstance",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.ParallelMultiInstance ? true : false
            };
            DiagramMenuItem l4 = new DiagramMenuItem()
            {
                Content = "SequenceMultiInstance",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.SequenceMultiInstance ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l4);


            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
            if (node.ActivityType == ActivityType.Task)
            {
                DiagramMenuItem tt = new DiagramMenuItem()
                {
                    Content = "TaskType",
                };
                DiagramMenuItem t1 = new DiagramMenuItem()
                {
                    Content = "None",
                    IsCheckable = node.TaskType == TaskType.None ? true : false
                };
                DiagramMenuItem t2 = new DiagramMenuItem()
                {
                    Content = "Service",
                    IsCheckable = node.TaskType == TaskType.Service ? true : false
                };
                DiagramMenuItem t3 = new DiagramMenuItem()
                {
                    Content = "Receive",
                    IsCheckable = node.TaskType == TaskType.Receive ? true : false
                };
                DiagramMenuItem t4 = new DiagramMenuItem()
                {
                    Content = "Send",
                    IsCheckable = node.TaskType == TaskType.Send ? true : false
                };
                DiagramMenuItem t5 = new DiagramMenuItem()
                {
                    Content = "InstantiatingReceive",
                    IsCheckable = node.TaskType == TaskType.InstantiatingReceive ? true : false
                };
                DiagramMenuItem t6 = new DiagramMenuItem()
                {
                    Content = "Manual",
                    IsCheckable = node.TaskType == TaskType.Manual ? true : false
                };
                DiagramMenuItem t7 = new DiagramMenuItem()
                {
                    Content = "BusinessRule",
                    IsCheckable = node.TaskType == TaskType.BusinessRule ? true : false
                };
                DiagramMenuItem t8 = new DiagramMenuItem()
                {
                    Content = "User",
                    IsCheckable = node.TaskType == TaskType.User ? true : false
                };
                DiagramMenuItem t9 = new DiagramMenuItem()
                {
                    Content = "Script",
                    IsCheckable = node.TaskType == TaskType.Script ? true : false
                };
                tt.Items = new ObservableCollection<DiagramMenuItem>()
            {
                t1,t2,t3,t4,t5,t6,t7,t8,t9
            };
                (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(tt);
            }
            else
            {
                DiagramMenuItem adhoc = new DiagramMenuItem()
                {
                    Content = "IsAdhoc",
                    IsCheckable = node.IsAdhocActivity ? true : false
                };
                (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(adhoc);
            }
            DiagramMenuItem comp = new DiagramMenuItem()
            {
                Content = "IsCompensation",
                IsCheckable = node.IsCompensationActivity ? true : false,
            };
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(comp);
            if (node.ActivityType == ActivityType.Task)
            {
                DiagramMenuItem call = new DiagramMenuItem()
                {
                    Content = "IsCall",
                    IsCheckable = node.IsCallActivity ? true : false,
                };
                (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(call);
            }
            else
            {
                DiagramMenuItem bound = new DiagramMenuItem()
                {
                    Content = "Boundary",

                };
                DiagramMenuItem b1 = new DiagramMenuItem()
                {
                    Content = "Call",
                    IsCheckable = node.SubProcessType == SubProcessType.Call ? true : false,
                };
                DiagramMenuItem b2 = new DiagramMenuItem()
                {
                    Content = "Default",
                    IsCheckable = node.SubProcessType == SubProcessType.Default ? true : false,
                };
                DiagramMenuItem b3 = new DiagramMenuItem()
                {
                    Content = "Event",
                    IsCheckable = node.SubProcessType == SubProcessType.Event ? true : false,
                };
                DiagramMenuItem b4 = new DiagramMenuItem()
                {
                    Content = "Transaction",
                    IsCheckable = node.SubProcessType == SubProcessType.Transaction ? true : false,
                };

                bound.Items = new ObservableCollection<DiagramMenuItem>()
                {
                    b1,b2,b3,b4
                };
                (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(bound);
            }
            DiagramMenuItem at = new DiagramMenuItem()
            {
                Content = "ActivityType",

            };
            DiagramMenuItem at1 = new DiagramMenuItem()
            {
                Content = "Task",
                IsCheckable = node.ActivityType == ActivityType.Task ? true : false,
            };
            DiagramMenuItem at2 = new DiagramMenuItem()
            {
                Content = "CollapsedSubProcess",
                IsCheckable = node.ActivityType == ActivityType.CollapsedSubProcess ? true : false,
            };
            at.Items = new ObservableCollection<DiagramMenuItem>()
            {
              at1,at2
            };

            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(at);
        }

        private void UpdateActivityShapeValue(BpmnNodeViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                if (element.Items != null)
                {
                    foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                    {
                        item.IsCheckable = false;
                    }
                }
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCall" ||
                (menuitem.Item as DiagramMenuItem).Content.ToString() == "IsAdhoc" ||
                (menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCompensation")
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCall")
                {
                    (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsCallActivity ? false : true;
                    node.IsCallActivity = (menuitem.Item as DiagramMenuItem).IsCheckable;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsAdhoc")
                {
                    (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsAdhocActivity ? false : true;
                    node.IsAdhocActivity = (menuitem.Item as DiagramMenuItem).IsCheckable;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCompensation")
                {
                    (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsCompensationActivity ? false : true;
                    node.IsCompensationActivity = (menuitem.Item as DiagramMenuItem).IsCheckable;
                }
            }
            else
            {
                (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            }
            if ((menuitem.Item as DiagramMenuItem).CommandParameter != null &&
                (menuitem.Item as DiagramMenuItem).CommandParameter.ToString() == "Loop")
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
                {
                    node.LoopActivity = LoopCharacteristic.None;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ParallelMultiInstance")
                {
                    node.LoopActivity = LoopCharacteristic.ParallelMultiInstance;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "SequenceMultiInstance")
                {
                    node.LoopActivity = LoopCharacteristic.SequenceMultiInstance;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Standard")
                {
                    node.LoopActivity = LoopCharacteristic.Standard;
                }
            }
            else
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
                {
                    node.TaskType = TaskType.None;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "BusinessRule")
                {
                    node.TaskType = TaskType.BusinessRule;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "InstantiatingReceive")
                {
                    node.TaskType = TaskType.InstantiatingReceive;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Manual")
                {
                    node.TaskType = TaskType.Manual;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Receive")
                {
                    node.TaskType = TaskType.Receive;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Script")
                {
                    node.TaskType = TaskType.Script;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Send")
                {
                    node.TaskType = TaskType.Send;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Service")
                {
                    node.TaskType = TaskType.Service;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "User")
                {
                    node.TaskType = TaskType.User;
                }
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Task")
            {
                node.ActivityType = ActivityType.Task;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "CollapsedSubProcess")
            {
                node.ActivityType = ActivityType.CollapsedSubProcess;
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Default")
            {
                node.SubProcessType = SubProcessType.Default;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Call")
            {
                node.SubProcessType = SubProcessType.Call;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Event")
            {
                node.SubProcessType = SubProcessType.Event;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Transaction")
            {
                node.SubProcessType = SubProcessType.Transaction;
            }
        }
        #endregion

        #region Gateway
        private void AddGatewayMenuItems(BpmnNodeViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "Gateway",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem l1 = new DiagramMenuItem()
            {
                Content = "None",
                IsCheckable = node.GatewayType == GatewayType.None ? true : false
            };
            DiagramMenuItem l2 = new DiagramMenuItem()
            {
                Content = "Complex",
                IsCheckable = node.GatewayType == GatewayType.Complex ? true : false
            };
            DiagramMenuItem l3 = new DiagramMenuItem()
            {
                Content = "EventBased",
                IsCheckable = node.GatewayType == GatewayType.EventBased ? true : false
            };
            DiagramMenuItem l4 = new DiagramMenuItem()
            {
                Content = "Exclusive",
                IsCheckable = node.GatewayType == GatewayType.Exclusive ? true : false
            };
            DiagramMenuItem l5 = new DiagramMenuItem()
            {
                Content = "ExclusiveEventBased",
                IsCheckable = node.GatewayType == GatewayType.ExclusiveEventBased ? true : false
            };
            DiagramMenuItem l6 = new DiagramMenuItem()
            {
                Content = "Inclusive",
                IsCheckable = node.GatewayType == GatewayType.Inclusive ? true : false
            };
            DiagramMenuItem l7 = new DiagramMenuItem()
            {
                Content = "Parallel",
                IsCheckable = node.GatewayType == GatewayType.Parallel ? true : false
            };
            DiagramMenuItem l8 = new DiagramMenuItem()
            {
                Content = "ParallelEventBased",
                IsCheckable = node.GatewayType == GatewayType.ParallelEventBased ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l4);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l5);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l6);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l7);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l8);

            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
        }

        private void UpdateGatewayShapeValue(BpmnNodeViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                if (element.Items != null)
                {
                    foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                    {
                        item.IsCheckable = false;
                    }
                }
            }
           (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
            {
                node.GatewayType = GatewayType.None;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Complex")
            {
                node.GatewayType = GatewayType.Complex;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "EventBased")
            {
                node.GatewayType = GatewayType.EventBased;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Exclusive")
            {
                node.GatewayType = GatewayType.Exclusive;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ExclusiveEventBased")
            {
                node.GatewayType = GatewayType.ExclusiveEventBased;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Inclusive")
            {
                node.GatewayType = GatewayType.Inclusive;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Parallel")
            {
                node.GatewayType = GatewayType.Parallel;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ParallelEventBased")
            {
                node.GatewayType = GatewayType.ParallelEventBased;
            }
        }
        #endregion

        #region Event
        private void AddEventMenuItems(BpmnNodeViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            //node.Constraints = node.Constraints & ~NodeConstraints.InheritMenu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "EventType",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem m1 = new DiagramMenuItem()
            {
                Content = "Start",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.Start ? true : false
            };
            DiagramMenuItem m2 = new DiagramMenuItem()
            {
                Content = "End",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.End ? true : false
            };
            DiagramMenuItem m3 = new DiagramMenuItem()
            {
                Content = "Intermediate",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.Intermediate ? true : false
            };
            DiagramMenuItem m4 = new DiagramMenuItem()
            {
                Content = "NonInterruptingIntermediate",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.NonInterruptingIntermediate ? true : false
            };
            DiagramMenuItem m5 = new DiagramMenuItem()
            {
                Content = "NonInterruptingStart",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.NonInterruptingStart ? true : false
            };
            DiagramMenuItem m6 = new DiagramMenuItem()
            {
                Content = "ThrowingIntermediate",
                CommandParameter = "EventType",
                IsCheckable = node.EventType == EventType.ThrowingIntermediate ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m4);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m5);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m6);
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
            DiagramMenuItem ti = new DiagramMenuItem()
            {
                Content = "EventTrigger",
            };
            ti.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem t1 = new DiagramMenuItem()
            {
                Content = "None",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.None ? true : false
            };
            DiagramMenuItem t2 = new DiagramMenuItem()
            {
                Content = "Message",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message ? true : false
            };
            DiagramMenuItem t3 = new DiagramMenuItem()
            {
                Content = "Cancel",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Cancel ? true : false
            };
            DiagramMenuItem t4 = new DiagramMenuItem()
            {
                Content = "Compensation",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Compensation ? true : false
            };
            DiagramMenuItem t5 = new DiagramMenuItem()
            {
                Content = "Conditional",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Conditional ? true : false
            };
            DiagramMenuItem t6 = new DiagramMenuItem()
            {
                Content = "Error",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Error ? true : false
            };
            DiagramMenuItem t7 = new DiagramMenuItem()
            {
                Content = "Escalation",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Escalation ? true : false
            };
            DiagramMenuItem t8 = new DiagramMenuItem()
            {
                Content = "Link",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Link ? true : false
            };
            DiagramMenuItem t9 = new DiagramMenuItem()
            {
                Content = "Multiple",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Multiple ? true : false
            };
            DiagramMenuItem t10 = new DiagramMenuItem()
            {
                Content = "Parallel",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Parallel ? true : false
            };
            DiagramMenuItem t11 = new DiagramMenuItem()
            {
                Content = "Signal",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Signal ? true : false
            };
            DiagramMenuItem t12 = new DiagramMenuItem()
            {
                Content = "Termination",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Termination ? true : false
            };
            DiagramMenuItem t13 = new DiagramMenuItem()
            {
                Content = "Timer",
                IsCheckable = node.EventTrigger == Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Timer ? true : false
            };
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t1);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t2);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t3);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t4);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t5);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t6);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t7);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t8);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t9);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t10);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t11);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t12);
            (ti.Items as ObservableCollection<DiagramMenuItem>).Add(t13);
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(ti);
        }
        private void UpdateEventShapeValue(BpmnNodeViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                {
                    item.IsCheckable = false;
                }
            }
            (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            if ((menuitem.Item as DiagramMenuItem).CommandParameter != null &&
               (menuitem.Item as DiagramMenuItem).CommandParameter.ToString() == "EventType")
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "End")
                {
                    node.EventType = EventType.End;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Intermediate")
                {
                    node.EventType = EventType.Intermediate;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "NonInterruptingIntermediate")
                {
                    node.EventType = EventType.NonInterruptingIntermediate;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "NonInterruptingStart")
                {
                    node.EventType = EventType.NonInterruptingStart;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Start")
                {
                    node.EventType = EventType.Start;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ThrowingIntermediate")
                {
                    node.EventType = EventType.ThrowingIntermediate;
                }
            }
            else
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Cancel")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Cancel;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Compensation")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Compensation;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Conditional")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Conditional;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Error")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Error;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Escalation")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Escalation;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Link")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Link;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Message")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Multiple")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Multiple;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.None;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Parallel")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Parallel;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Signal")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Signal;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Termination")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Termination;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Timer")
                {
                    node.EventTrigger = Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Timer;
                }
            }
        }
        #endregion

        #region DataObject
        private void AddDataObjectMenuItems(BpmnNodeViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            //node.Constraints = node.Constraints & ~NodeConstraints.InheritMenu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem ni = new DiagramMenuItem()
            {
                Content = "IsCollectiveData",
                IsCheckable = node.IsCollectiveData ? true : false
            };
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "DataObjectType",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem m1 = new DiagramMenuItem()
            {
                Content = "None",
                IsCheckable = node.DataObjectType == DataObjectType.None ? true : false
            };
            DiagramMenuItem m2 = new DiagramMenuItem()
            {
                Content = "Input",
                IsCheckable = node.DataObjectType == DataObjectType.Input ? true : false
            };
            DiagramMenuItem m3 = new DiagramMenuItem()
            {
                Content = "Output",
                IsCheckable = node.DataObjectType == DataObjectType.Output ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(m3);
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(ni);
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
        }
        private void UpdateDataObjectShapeValue(BpmnNodeViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                if (element.Items != null)
                {
                    foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                    {
                        item.IsCheckable = false;
                    }
                }
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCollectiveData")
            {
                (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsCollectiveData ? false : true;
                node.IsCollectiveData = (menuitem.Item as DiagramMenuItem).IsCheckable;
            }
            else
            {
                (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
            {
                node.DataObjectType = DataObjectType.None;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Input")
            {
                node.DataObjectType = DataObjectType.Input;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Output")
            {
                node.DataObjectType = DataObjectType.Output;
            }
        }
        #endregion

        #region ExpandedSubProcess

        private void AddExpandedSubProcessMenuItems(BpmnGroupViewModel node)
        {
            node.Constraints = node.Constraints | NodeConstraints.Menu;
            node.Menu = new DiagramMenu();
            node.Menu.MenuItems = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem mi = new DiagramMenuItem()
            {
                Content = "Loop",
            };
            mi.Items = new ObservableCollection<DiagramMenuItem>();
            DiagramMenuItem l1 = new DiagramMenuItem()
            {
                Content = "None",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.None ? true : false
            };
            DiagramMenuItem l2 = new DiagramMenuItem()
            {
                Content = "Standard",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.Standard ? true : false
            };
            DiagramMenuItem l3 = new DiagramMenuItem()
            {
                Content = "ParallelMultiInstance",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.ParallelMultiInstance ? true : false
            };
            DiagramMenuItem l4 = new DiagramMenuItem()
            {
                Content = "SequenceMultiInstance",
                CommandParameter = "Loop",
                IsCheckable = node.LoopActivity == LoopCharacteristic.SequenceMultiInstance ? true : false
            };
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l1);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l2);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l3);
            (mi.Items as ObservableCollection<DiagramMenuItem>).Add(l4);


            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(mi);
            DiagramMenuItem adhoc = new DiagramMenuItem()
            {
                Content = "IsAdhoc",
                IsCheckable = node.IsAdhocActivity ? true : false
            };
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(adhoc);
            DiagramMenuItem comp = new DiagramMenuItem()
            {
                Content = "IsCompensation",
                IsCheckable = node.IsCompensationActivity ? true : false,
            };
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(comp);
            DiagramMenuItem bound = new DiagramMenuItem()
            {
                Content = "Boundary",

            };
            DiagramMenuItem b1 = new DiagramMenuItem()
            {
                Content = "Call",
                IsCheckable = node.SubProcessType == SubProcessType.Call ? true : false,
            };
            DiagramMenuItem b2 = new DiagramMenuItem()
            {
                Content = "Default",
                IsCheckable = node.SubProcessType == SubProcessType.Default ? true : false,
            };
            DiagramMenuItem b3 = new DiagramMenuItem()
            {
                Content = "Event",
                IsCheckable = node.SubProcessType == SubProcessType.Event ? true : false,
            };
            DiagramMenuItem b4 = new DiagramMenuItem()
            {
                Content = "Transaction",
                IsCheckable = node.SubProcessType == SubProcessType.Transaction ? true : false,
            };

            bound.Items = new ObservableCollection<DiagramMenuItem>()
                {
                    b1,b2,b3,b4
                };
            (node.Menu.MenuItems as ICollection<DiagramMenuItem>).Add(bound);
        }

        private void UpdateExpandedSubProcessShapeValue(BpmnGroupViewModel node, MenuItemClickedEventArgs menuitem)
        {
            foreach (var element in node.Menu.MenuItems as ObservableCollection<DiagramMenuItem>)
            {
                if (element.Items != null)
                {
                    foreach (var item in element.Items as ObservableCollection<DiagramMenuItem>)
                    {
                        item.IsCheckable = false;
                    }
                }
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsAdhoc" ||
                (menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCompensation")
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsAdhoc")
                {
                    (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsAdhocActivity ? false : true;
                    node.IsAdhocActivity = (menuitem.Item as DiagramMenuItem).IsCheckable;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "IsCompensation")
                {
                    (menuitem.Item as DiagramMenuItem).IsCheckable = node.IsCompensationActivity ? false : true;
                    node.IsCompensationActivity = (menuitem.Item as DiagramMenuItem).IsCheckable;
                }
            }
            else
            {
                (menuitem.Item as DiagramMenuItem).IsCheckable = true;
            }
            if ((menuitem.Item as DiagramMenuItem).CommandParameter != null &&
                (menuitem.Item as DiagramMenuItem).CommandParameter.ToString() == "Loop")
            {
                if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "None")
                {
                    node.LoopActivity = LoopCharacteristic.None;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "ParallelMultiInstance")
                {
                    node.LoopActivity = LoopCharacteristic.ParallelMultiInstance;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "SequenceMultiInstance")
                {
                    node.LoopActivity = LoopCharacteristic.SequenceMultiInstance;
                }
                else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Standard")
                {
                    node.LoopActivity = LoopCharacteristic.Standard;
                }
            }
            if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Default")
            {
                node.SubProcessType = SubProcessType.Default;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Call")
            {
                node.SubProcessType = SubProcessType.Call;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Event")
            {
                node.SubProcessType = SubProcessType.Event;
            }
            else if ((menuitem.Item as DiagramMenuItem).Content.ToString() == "Transaction")
            {
                node.SubProcessType = SubProcessType.Transaction;
            }
        }
        #endregion

        /// <summary>
        /// This method is used to execute Save command
        /// </summary>
        /// <param name="obj"></param>
        private void OnSave(object obj)
        {
            SaveFileDialog saveAsFileDialog = new SaveFileDialog { Title = "Save ", DefaultExt = ".xml" };
            saveAsFileDialog.Filter = "Text file (*.xml)|*.xml";

            if (saveAsFileDialog.ShowDialog() == true)
            {
                this._SavedPath = saveAsFileDialog.FileName;
                IGraphInfo graph = this.Info as IGraphInfo;
                using (Stream fileStream = saveAsFileDialog.OpenFile())
                {
                    graph.Save(fileStream);
                }
            }
        }

        /// <summary>
        /// This method is used to execute load command
        /// </summary>
        /// <param name="obj"></param>
        private void OnLoad(object obj)
        {
            OpenFileDialog openDialogBox = new OpenFileDialog();
            openDialogBox.Title = "Load";
            openDialogBox.RestoreDirectory = true;
            openDialogBox.DefaultExt = "xml";
            openDialogBox.Filter = "All supported files (*.xml, *.bpmn)|*.xml;*.bpmn|xml files (*.xml)|*.xml|bpmn diagrams (*.bpmn)|*.bpmn";
            bool? isFileChosen = openDialogBox.ShowDialog();
            if (isFileChosen == true)
            {
                this._SavedPath = openDialogBox.FileName;

                var fileInfo = new System.IO.FileInfo(this._SavedPath);
                if (fileInfo.Extension == ".bpmn")
                {
                    // Load the bpmn document
                    using (FileStream fileStream = File.OpenRead(this._SavedPath))
                    {
                        //var simulator = new Simulator(fileStream);

                        //var simulatorInstance = simulator.NewProcessInstance();
                        //simulatorInstance.SetDefaultHandlers();
                        //simulatorInstance.SetHandler("task", new ActivityHandler());
                        //simulatorInstance.SetHandler("startEvent", new StartHandler());
                        //simulatorInstance.SetHandler("inclusiveGateway", new GatewayHandler());
                        //simulatorInstance.SetHandler("exclusiveGateway", new GatewayHandler());
                        //simulatorInstance.SetHandler("parallelGateway", new GatewayHandler());
                        //simulatorInstance.SetHandler("endEvent", new StartHandler());
                        //simulatorInstance.SetHandler("sequenceFlow", new FlowHandler());

                        ////add process variables here
                        ////var processVar = new Dictionary<string, object>() { { "processVar1", "value" }, { "processVar2", 50 } };
                        //var processVar = new Dictionary<string, object>();
                        //simulatorInstance.Start(processVar);
                        //Console.ReadLine();

                        var simulator = FaaSDES.Sim.Simulator.FromBpmnXML(fileStream);

                        var tokenGenerator = new TimerSimTokenGenerator(
                            new GenerationSettings(0, 5),
                            new TimeOnly(8,0),
                            new TimeOnly(15,0),
                            new WeekDaySchedule(true, true, true, true, true, false, false));

                        var simSettings = new Sim.SimulationSettings()
                        {
                            StartDateTime = new DateTime(2021, 01, 01),
                            EndDateTime = new DateTime(2023, 12, 31),
                            MaximumIterations = int.MaxValue,
                            TimeFactor = new TimeSpan(0, 1, 0),
                            TokenMaxQueueTime = new TimeSpan(1, 30, 0)
                        };

                        
                        var simulation = simulator.NewSimulationInstance(simSettings, tokenGenerator);

                        //simulation.Execute();


                    }

                    var url = "http://localhost:7071/api/ExecuteSimulation";
                    var client = new RestSharp.RestClient(url);
                    var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                    //request.AddHeader("x-functions-key", securityCode);
                    //request.AddQueryParameter("ResourceGroupName", "ImageStormSource");

                    request.AddParameter("text/xml", File.ReadAllText(this._SavedPath), RestSharp.ParameterType.RequestBody);


                    var response = client.Execute(request);
                }
                else
                {
                    // Load the app specific XML format
                    using (FileStream fileStream = File.OpenRead(this._SavedPath))
                    {
                        (this.Info as IGraphInfo).Load(fileStream);
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to execute new command
        /// </summary>
        /// <param name="obj"></param>
        private void OnNew(object obj)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(
                       "Do you want to save Diagram?",
                       "BpmnEditor Diagram",
                       MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.OnSave(null);
            }
            (this.Nodes as ObservableCollection<NodeViewModel>).Clear();
            (this.Connectors as ObservableCollection<ConnectorViewModel>).Clear();
        }

        #endregion

        private void InitializeDiagram()
        {
            FaaSDESEventNodeViewModel eventstart = CreateEventNode(100, 300, EventType.Start, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.None);
            FaaSDESActivityNodeViewModel task1 = CreateTaskNode(250, 300, TaskType.Receive, "Receive Book lending Request");
            FaaSDESActivityNodeViewModel task2 = CreateTaskNode(420, 300, TaskType.Service, "Get the Book Status");
            FaaSDESGatewayViewModel gateway1 = CreateGatewayNode(570, 300, GatewayType.None);
            NodePortViewModel port1 = CreatePort(0.5, 1, gateway1);
            FaaSDESActivityNodeViewModel task3 = CreateTaskNode(780, 300, TaskType.Send, "On Loan Reply");
            FaaSDESActivityNodeViewModel task4 = CreateTaskNode(780, 550, TaskType.User, "Checkout the Book");
            NodePortViewModel port2 = CreatePort(0, 0.5, task4);
            FaaSDESGatewayViewModel gateway2 = CreateGatewayNode(920, 300, GatewayType.ExclusiveEventBased);
            NodePortViewModel port3 = CreatePort(0.5, 0, gateway2);
            NodePortViewModel port4 = CreatePort(0.5, 1, gateway2);
            FaaSDESEventNodeViewModel event1 = CreateEventNode(1050, 200, EventType.Intermediate, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message, "Hold Book");
            NodePortViewModel port5 = CreatePort(0, 0.5, event1);
            FaaSDESEventNodeViewModel event2 = CreateEventNode(1050, 300, EventType.Intermediate, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message, "Decline Hold");
            FaaSDESEventNodeViewModel event3 = CreateEventNode(1050, 400, EventType.Intermediate, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message, "One Week");
            NodePortViewModel port6 = CreatePort(0, 0.5, event3);
            NodePortViewModel port7 = CreatePort(1, 0.5, event3);
            FaaSDESActivityNodeViewModel task5 = CreateTaskNode(1050, 550, TaskType.Receive, "Checkout Reply");
            NodePortViewModel port9 = CreatePort(1, 0.5, task5);
            FaaSDESActivityNodeViewModel task6 = CreateTaskNode(1200, 200, TaskType.Service, "Request Hold");
            FaaSDESActivityNodeViewModel task7 = CreateTaskNode(1200, 300, TaskType.Receive, "Cancel Request");
            NodePortViewModel port8 = CreatePort(0.5, 1, task7);
            FaaSDESActivityNodeViewModel task8 = CreateTaskNode(1400, 200, TaskType.Receive, "Hold Reply");
            NodePortViewModel port11 = CreatePort(0.5, 0, task8);
            FaaSDESEventNodeViewModel event4 = CreateEventNode(1400, 300, EventType.End, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.None);
            NodePortViewModel port10 = CreatePort(0.5, 1, event4);
            FaaSDESEventNodeViewModel event5 = CreateEventNode(900, 70, EventType.Intermediate, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger.Message, "Two Weeks");
            NodePortViewModel port12 = CreatePort(1, 0.5, event5);
            NodePortViewModel port13 = CreatePort(0, 0.5, event5);
            NodePortViewModel port14 = CreatePort(0.5, 0, task2);

            CreateBpmnFlow(eventstart, task1);
            CreateBpmnFlow(task1, task2);
            CreateBpmnFlow(task2, gateway1);
            CreateBpmnFlow(gateway1, task3, "Book is on Loan");
            CreateBpmnFlow(gateway1, task4, "Book is Available", port1, port2);
            CreateBpmnFlow(task3, gateway2);
            CreateBpmnFlow(task4, task5);
            CreateBpmnFlow(gateway2, event1, null, port3, port5);
            CreateBpmnFlow(gateway2, event2);
            CreateBpmnFlow(gateway2, event3, null, port4, port6);
            CreateBpmnFlow(event1, task6);
            CreateBpmnFlow(event2, task7);
            CreateBpmnFlow(event3, task7, null, port7, port8);
            CreateBpmnFlow(task6, task8);
            CreateBpmnFlow(task7, event4);
            CreateBpmnFlow(task5, event4, null, port9, port10);
            CreateBpmnFlow(task8, event5, null, port11, port12);
            CreateBpmnFlow(event5, task2, null, port13, port14);
        }

        private NodePortViewModel CreatePort(double offx, double offy, BpmnNodeViewModel node)
        {
            NodePortViewModel port = new()
            {
                NodeOffsetX = offx,
                NodeOffsetY = offy,
                Node = node,
            };
            if (node.Ports == null)
            {
                node.Ports = new PortCollection();
            }
            (node.Ports as PortCollection).Add(port);
            return port;
        }

        private FaaSDESFlowViewModel CreateBpmnFlow(BpmnNodeViewModel snode, BpmnNodeViewModel tnode, string content = null, NodePortViewModel p1 = null, NodePortViewModel p2 = null)
        {
            FaaSDESFlowViewModel con = new FaaSDESFlowViewModel()
            {
                SourceNode = snode,
                TargetNode = tnode,
                FlowType = BpmnFlowType.SequenceFlow,
            };
            if (p1 != null)
            {
                con.SourcePort = p1;
            }
            if (p2 != null)
            {
                con.TargetPort = p2;
            }
            if (content != null)
            {
                AnnotationEditorViewModel anno = new AnnotationEditorViewModel()
                {
                    Content = content,
                    UnitHeight = 40,
                    UnitWidth = 60,
                };
                anno.ViewTemplate = AnnotationTemplate.GetViewTemplate();
                anno.EditTemplate = AnnotationTemplate.GetEditTemplate();
                if (content == "Book is Available")
                {
                    anno.RotationReference = RotationReference.Page;
                }
                con.Annotations = new ObservableCollection<IAnnotation>()
                {
                  anno
                };
            }
            (this.Connectors as ObservableCollection<ConnectorViewModel>).Add(con);
            return con;
        }

        private FaaSDESGatewayViewModel CreateGatewayNode(double offx, double offy, GatewayType gatewaytype)
        {
            FaaSDESGatewayViewModel gatewaynode = new()
            {
                OffsetX = offx,
                OffsetY = offy,
                UnitHeight = 72,
                UnitWidth = 96,
                Type = BpmnShapeType.Gateway,
                GatewayType = gatewaytype
            };
            (this.Nodes as ObservableCollection<NodeViewModel>).Add(gatewaynode);
            return gatewaynode;
        }

        private FaaSDESActivityNodeViewModel CreateTaskNode(double offx, double offy, TaskType taskType, string content)
        {
            FaaSDESActivityNodeViewModel tasknode = new()
            {
                OffsetX = offx,
                OffsetY = offy,
                UnitHeight = 70,
                UnitWidth = 120,
                Type = BpmnShapeType.Activity,
                TaskType = taskType
            };
            tasknode.Annotations = new ObservableCollection<IAnnotation>()
            {
                new AnnotationEditorViewModel()
                {
                    Content=content,
                }
            };
            (this.Nodes as ObservableCollection<NodeViewModel>).Add(tasknode);
            return tasknode;
        }

        private FaaSDESEventNodeViewModel CreateEventNode(double offx, double offy, EventType eventtype, Syncfusion.UI.Xaml.Diagram.Controls.EventTrigger eventtrigger, string content = null)
        {
            FaaSDESEventNodeViewModel eventnode = new()
            {
                OffsetX = offx,
                OffsetY = offy,
                UnitHeight = 50,
                UnitWidth = 50,
                Type = BpmnShapeType.Event,
                EventType = eventtype,
                EventTrigger = eventtrigger,
            };
            if (content != null)
            {
                eventnode.Annotations = new ObservableCollection<IAnnotation>()
            {
                new AnnotationEditorViewModel()
                {
                    Content=content,
                    WrapText=TextWrapping.NoWrap,
                    UnitHeight = 30,
                    UnitWidth = 200,
                    Offset=new Point(0.5,1),
                    Margin=new Thickness(0,10,0,0),
                    TextVerticalAlignment=VerticalAlignment.Center,
                    TextHorizontalAlignment=TextAlignment.Center,
                    VerticalAlignment=VerticalAlignment.Top
                }
            };
            }
            (this.Nodes as ObservableCollection<NodeViewModel>).Add(eventnode);
            return eventnode;
        }

        public void OnItemAddedCommandCommand(object parameter)
        {
            var element = parameter as ItemAddedEventArgs;
            if (element != null)
            {
                var item = element.Item as ConnectorViewModel;
                if (item != null)
                {
                    var anno = (item.Annotations as ObservableCollection<IAnnotation>).FirstOrDefault() as AnnotationEditorViewModel;
                    if (anno != null)
                    {
                        anno.ViewTemplate = AnnotationTemplate.GetViewTemplate();
                        anno.EditTemplate = AnnotationTemplate.GetEditTemplate();
                    }
                }
            }
        }

        public void OnButtonMenuOpeningCommand(object parameter)
        {
            Button source = parameter as Button;
            if (source != null && source.ContextMenu != null)
            {
                source.ContextMenu.PlacementTarget = source;
                source.ContextMenu.Placement = PlacementMode.Bottom;
                source.ContextMenu.IsOpen = true;
            }
        }

        private void OnOrientationCommand(object parameter)
        {
            if (parameter != null)
            {
                if (parameter.ToString() == "Landscape")
                {
                    this.PageSettings.PageOrientation = PageOrientation.Landscape;
                    this.LandscapeProperty = true;
                    this.PortraitProperty = false;
                }
                else
                {
                    this.PageSettings.PageOrientation = PageOrientation.Portrait;
                    this.LandscapeProperty = false;
                    this.PortraitProperty = true;
                }
            }
        }

        private void OnLoadBlankDiagramCommand(object parameter)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show(
                     "Do you want to save Diagram?",
                     "BpmnEditor Diagram",
                     MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.OnSave(null);
            }

            (this.Nodes as ObservableCollection<NodeViewModel>).Clear();
            (this.Connectors as ObservableCollection<ConnectorViewModel>).Clear();
        }

        private void OnChangeConnectorTypeCommand(object parameter)
        {
            this.StraightProperty = OrthogonalProperty = BezierProperty = false;
            string paramValue = parameter != null ? parameter.ToString() : null;
            if (paramValue != null)
            {
                switch (paramValue)
                {
                    case "Orthogonal":
                        {
                            this.DefaultConnectorType = ConnectorType.Orthogonal;
                            this.OrthogonalProperty = true;
                            foreach (var item in (this.SelectedItems as SelectorViewModel).Connectors as IEnumerable<object>)
                            {
                                (item as IConnector).Segments = new ObservableCollection<IConnectorSegment>()
                            {
                                new OrthogonalSegment()
                                {
                                }
                            };
                            }
                        }
                        break;
                    case "Straight":
                        {
                            this.DefaultConnectorType = ConnectorType.Line;
                            this.StraightProperty = true;
                            foreach (var item in (this.SelectedItems as SelectorViewModel).Connectors as IEnumerable<object>)
                            {
                                (item as IConnector).Segments = new ObservableCollection<IConnectorSegment>()
                            {
                                new StraightSegment()
                                {
                                }
                            };
                            }
                        }
                        break;
                    case "Bezier":
                        {
                            this.DefaultConnectorType = ConnectorType.CubicBezier;
                            this.BezierProperty = true;
                            foreach (var item in (this.SelectedItems as SelectorViewModel).Connectors as IEnumerable<object>)
                            {
                                (item as IConnector).Segments = new ObservableCollection<IConnectorSegment>()
                            {
                                new CubicCurveSegment()
                                {
                                }
                            };
                            }
                        }
                        break;
                }
            }
        }

        private void OnSelectAllConnectorsCommand(object parameter)
        {
            ((this.SelectedItems as SelectorViewModel).Nodes as ObservableCollection<object>).Clear();
            ((this.SelectedItems as SelectorViewModel).Connectors as ObservableCollection<object>).Clear();
            foreach (IConnector connector in this.Connectors as IEnumerable<object>)
            {
                connector.IsSelected = true;
            }
        }

        private void OnSelectAllNodesCommand(object parameter)
        {
            ((this.SelectedItems as SelectorViewModel).Nodes as ObservableCollection<object>).Clear();
            ((this.SelectedItems as SelectorViewModel).Connectors as ObservableCollection<object>).Clear();
            foreach (NodeViewModel node in this.Nodes as IEnumerable<object>)
            {
                node.IsSelected = true;
            }
        }

        public void OnViewPortChangedCommand(object parameter)
        {
            int val = Convert.ToInt32(this.ScrollSettings.ScrollInfo.CurrentZoom * 100);
            CurrentZoom = this.ScrollSettings.ScrollInfo.CurrentZoom;
            ZoomPercentageValue = val.ToString();
            var args = parameter as ChangeEventArgs<object, ScrollChanged>;
            if (args.NewValue.CurrentZoom >= 3)
            {
                ZoomInEnabled = false;
            }
            else
            {
                ZoomInEnabled = true;
            }
            if (args.NewValue.CurrentZoom == 0.3)
            {
                ZoomOutEnabled = false;
            }
            else
            {
                ZoomOutEnabled = true;
            }
        }

        public void OnFitToWidthCommand(object parameter)
        {
            (this.Info as IGraphInfo).Commands.FitToPage.Execute(new FitToPageParameter { FitToPage = FitToPage.FitToWidth, Region = Region.PageSettings });
        }

        public void OnFitToPageCommand(object parameter)
        {
            (this.Info as IGraphInfo).Commands.FitToPage.Execute(null);
        }

        private void OnItemUnSelectedCommand(object parameter)
        {
            if (((this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>).Count() > 0 ||
                ((this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>).Count() > 0)
            {
                IsItemSelected = true;
            }
            else
            {
                IsItemSelected = false;
            }
        }

        public void OnItemSelectedCommand(object parameter)
        {
            if (((this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>).Count() > 0 ||
               ((this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>).Count() > 0)
            {
                IsItemSelected = true;
            }
            else
            {
                IsItemSelected = false;
            }
        }

        public void OnShowRulerCommand(object parameter)
        {
            if ((bool)parameter)
            {
                this.HorizontalRuler = new Ruler();
                this.VerticalRuler = new Ruler() { Orientation = Orientation.Vertical };
            }
            else
            {
                this.HorizontalRuler = null;
                this.VerticalRuler = null;
            }
        }

        public void OnShowLinesCommand(object parameter)
        {
            if ((bool)parameter)
                this.SnapSettings.SnapConstraints |= SnapConstraints.ShowLines;
            else
                this.SnapSettings.SnapConstraints &= ~SnapConstraints.ShowLines;
        }

        public void OnShowPageBreaksCommand(object parameter)
        {
            if ((bool)parameter)
                this.PageSettings.ShowPageBreaks = true;
            else
                this.PageSettings.ShowPageBreaks = false;
        }

        public void OnSnapToGridCommand(object parameter)
        {
            if ((bool)parameter)
                this.SnapSettings.SnapConstraints |= SnapConstraints.SnapToLines;
            else
                this.SnapSettings.SnapConstraints &= ~SnapConstraints.SnapToLines;
        }

        public void OnSnapToObjectCommand(object parameter)
        {
            if ((bool)parameter)
                this.SnapSettings.SnapToObject = SnapToObject.All;
            else
                this.SnapSettings.SnapToObject = SnapToObject.None;
        }

        public void OnZoomOutCommand(object parameter)
        {
            (this.Info as IGraphInfo).Commands.Zoom.Execute(new ZoomPositionParameter()
            {
                ZoomCommand = ZoomCommand.ZoomOut,
                ZoomFactor = 0.2,
            });
        }

        public void OnZoomInCommand(object parameter)
        {
            double zoomValue = (this.ScrollSettings.ScrollInfo.CurrentZoom * 0.2d) + this.ScrollSettings.ScrollInfo.CurrentZoom;
            if (zoomValue > 3)
            {
                (this.Info as IGraphInfo).Commands.Zoom.Execute(new ZoomPositionParameter()
                {
                    ZoomCommand = ZoomCommand.Zoom,
                    ZoomTo = 3,
                });
            }
            else
            {
                (this.Info as IGraphInfo).Commands.Zoom.Execute(new ZoomPositionParameter()
                {
                    ZoomCommand = ZoomCommand.ZoomIn,
                    ZoomFactor = 0.2,
                });
            }
        }

        public void OnSelectNoneCommand(object parameter)
        {
            ((this.SelectedItems as SelectorViewModel).Nodes as ObservableCollection<object>).Clear();
            ((this.SelectedItems as SelectorViewModel).Connectors as ObservableCollection<object>).Clear();
        }

        public void OnRotateCounterColockwiseCommand(object parameter)
        {
            foreach (var item in (this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>)
            {
                (item as INode).RotateAngle -= 90;
            }
        }

        public void OnRotateColockwiseCommand(object parameter)
        {
            foreach (var item in (this.SelectedItems as SelectorViewModel).Nodes as IEnumerable<object>)
            {
                (item as INode).RotateAngle += 90;
            }
        }

        public void OnPanToolCommand(object parameter)
        {
            if (parameter != null)
            {
                if (!(bool)parameter)
                {
                    this.Tool = Tool.ZoomPan;
                    this.SelectToolProperty = false;
                    this.DrawToolProperty = false;
                    this.PanToolProperty = true;
                    this.IsPanToolSelected = true;
                    this.IsSelectToolSelected = false;
                }
                else
                {
                    this.IsPanToolSelected = false;
                }
            }
        }

        public void OnDrawConnectorCommand(object parameter)
        {
            if (parameter != null)
            {
                if ((bool)parameter)
                {
                    this.Tool = Tool.ContinuesDraw;
                    this.DrawToolProperty = true;
                    this.SelectToolProperty = false;
                    this.PanToolProperty = false;
                    this.IsSelectToolSelected = false;
                    this.IsPanToolSelected = false;
                }
                else
                {
                    this.DrawToolProperty = false;
                }
            }
        }

        public void OnSelectToolCommand(object parameter)
        {
            if (parameter != null)
            {
                if (!(bool)parameter)
                {
                    this.Tool = Tool.MultipleSelect;
                    this.SelectToolProperty = true;
                    this.DrawToolProperty = false;
                    this.PanToolProperty = false;
                    this.IsSelectToolSelected = true;
                    this.IsPanToolSelected = false;
                }
                else
                {
                    SelectToolProperty = false;
                }
            }
        }

        public void OnPrintCommand(object parameter)
        {
            PrintingService.ShowDialog = true;
            PrintingService.Print();
        }

        public void OnExportCommand(object parameter)
        {
            String Extension = "BMP File (*.bmp)|*.bmp|WDP File (*.wdp)|*.wdp|JPG File(*.jpg)|*.jpg|PNG File(*.png)|*.png|TIF File(*.tif)|*.tif|GIF File(*.gif)|*.gif|XPS File(*.xps)|*.xps|PDF File(*.pdf)|*.pdf";

            //To Represent SaveFile Dialog Box
            SaveFileDialog m_SaveFileDialog = new SaveFileDialog();

            //Assign the selected extension to the SavefileDialog filter
            m_SaveFileDialog.Filter = Extension;

            //To display savefiledialog       
            bool? istrue = m_SaveFileDialog.ShowDialog();
            string filenamechanged;

            if (istrue == true)
            {
                //assign the filename to a local variable
                string extension = System.IO.Path.GetExtension(m_SaveFileDialog.FileName).TrimStart('.');
                string fileName = m_SaveFileDialog.FileName;
                if (extension != "")
                {
                    if (extension.ToLower() == "pdf")
                    {
                        filenamechanged = fileName + ".xps";

                        ExportSettings.IsSaveToXps = true;

                        //Assigning exportstream from the saved file
                        this.ExportSettings.FileName = filenamechanged;
                        // Method to Export the SfDiagram
                        (this.Info as IGraphInfo).Export();

                        var converter = new XPSToPdfConverter { };

                        var document = new PdfDocument();

                        document = converter.Convert(filenamechanged);
                        document.Save(fileName);
                        document.Close(true);

                    }
                    {
                        if (extension.ToLower() == "xps")
                        {
                            ExportSettings.IsSaveToXps = true;
                        }
                        //Assigning exportstream from the saved file
                        this.ExportSettings.FileName = fileName;
                        // Method to Export the SfDiagram
                        (this.Info as IGraphInfo).Export();
                    }
                }
            }
        }

        public void OnSelectionChangedCommand(object parameter)
        {
            if (parameter != null)
            {
                if (parameter.GetType() == typeof(ItemSelectedEventArgs))
                {
                    var args = (ItemSelectedEventArgs)parameter;

                    if (args.Item.GetType() == typeof(FaaSDESNodeViewModel) ||
                        args.Item.GetType() == typeof(FaaSDESActivityNodeViewModel) ||
                        args.Item.GetType() == typeof(FaaSDESEventNodeViewModel) ||
                        args.Item.GetType() == typeof(FaaSDESGatewayViewModel))
                        SelectedFaaSDESActivityNodeViewModel = (FaaSDESNodeViewModel)args.Item;
                }
            }
        }
    }

    public static class AnnotationTemplate
    {
        public static DataTemplate GetViewTemplate()
        {
            const string vTemplate = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                      "<Border Background=\"White\" >" +
                                      "<TextBlock  TextAlignment=\"Center\"" +
                                                 " TextWrapping=\"{Binding Path = WrapText, Mode = OneWay}\"" +
                                                 " FontFamily=\"Arial\"" +
                                                 " FontSize=\"12\"" +
                                                 " HorizontalAlignment=\"Center\"" +
                                                 " VerticalAlignment=\"Center\"" +
                                                 " Foreground=\"Black\"" +
                                                 " Text=\"{Binding Path=Content, Mode=TwoWay}\"/>" +
                                      "</Border>" +
                                      "</DataTemplate>";

            return vTemplate.LoadXaml() as DataTemplate;
        }

        public static object LoadXaml(this string xaml)
        {
            return XamlReader.Parse(xaml);
        }

        public static DataTemplate GetEditTemplate()
        {
            const string eTemplate = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                      "<Border Background=\"White\">" +
                                      "<TextBox TextAlignment=\"Center\"" +
                                                 " TextWrapping=\"{Binding Path = WrapText, Mode = OneWay}\"" +
                                                 " FontFamily=\"Arial\"" +
                                                 " FontSize=\"12\"" +
                                                 " HorizontalAlignment=\"Center\"" +
                                                 " VerticalAlignment=\"Center\"" +
                                                 " Foreground=\"Black\"" +
                                                 " Text=\"{Binding Path=Content, Mode=TwoWay}\"/>" +
                                      "</Border>" +
                                      "</DataTemplate>";
            return eTemplate.LoadXaml() as DataTemplate;
        }
    }

    class StartHandler : INodeHandler
    {
        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeType);
            currentNode.Done();
        }
    }

    class ActivityHandler : INodeHandler
    {
        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine($"Executing {currentNode.Content}...");
            currentNode.Done();
        }
    }

    class GatewayHandler : INodeHandler
    {
        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine(currentNode.NodeName);
            currentNode.Done();
        }
    }

    class EndHandler : INodeHandler
    {
        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine($"Executing {currentNode.NodeType}...");
            currentNode.Done();
        }
    }

    class FlowHandler : INodeHandler
    {
        public void Execute(SimulationNode currentNode, SimulationNode previousNode)
        {
            Trace.WriteLine($"Executing flow from {previousNode.NodeName} to {currentNode.NextNodes.First().NodeName}");
            currentNode.Done();
        }
    }
}
