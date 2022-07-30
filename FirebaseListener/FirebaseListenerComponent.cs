using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Firebase.Database;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace FirebaseListener
{
    public class FirebaseListenerComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FirebaseListenerComponent()
          : base("Firebase Listener", "FBListen",
            "Listen for changes to a Firebase database",
            "Firebase", "Streaming")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Database URL", "U", "The URL for your realtime database", GH_ParamAccess.item);
            pManager.AddTextParameter("App Secret", "S", "Your app secret, to authenticate access to the database.", GH_ParamAccess.item);
            pManager.AddTextParameter("Key", "K", "The top-level data key to retrieve", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Listen", "L", "Set to true to enable live listening.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JSON Data", "J", "The data returned from the specified key", GH_ParamAccess.item);
        }

        private bool Listen = false;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string dbUrl = null;
            string appSecret = null;
            string key = null;
            DA.GetData("Database URL", ref dbUrl);
            DA.GetData("App Secret", ref appSecret);
            DA.GetData("Key", ref key);
            DA.GetData("Listen", ref Listen);

            if (Listen && !initialized)
            {
                Result = new JObject();
                // initialize
                client = new FirebaseClient(dbUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(appSecret)
                });
                var observable = client.Child(key).AsObservable<JObject>();
                subscription = observable.Subscribe(x =>
                {
                    if (!Listen)
                    {
                        return;
                    }
                    switch (x.EventType)
                    {
                        case Firebase.Database.Streaming.FirebaseEventType.Delete:
                            Result.Remove(x.Key);
                            break;
                        case Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate:
                            UpdateResult(x.Key, x.Object);
                            break;
                    }

                    OnPingDocument().ScheduleSolution(10, (GH_Document doc) =>
                    {
                        if (Listen)
                        {
                            this.ExpireSolution(false);
                        }
                    });
                });
                initialized = true;
            }
            else if (!Listen && initialized)
            {
                // dispose
                if (subscription != null)
                {
                    subscription.Dispose();
                }
                client.Dispose();
                client = null;
                initialized = false;
            }

            DA.SetData("JSON Data", Result.ToString());
        }

        private IDisposable subscription = null;

        private void UpdateResult(string key, JObject @object)
        {
            if (Result.ContainsKey(key) && @object.Children().Count() == 1)
            {
                // update each child key
                foreach (var child in @object)
                {
                    Result[key][child.Key] = child.Value;
                }
            }
            else
            {
                Result[key] = @object;
            }
        }

        private JObject Result = new JObject();

        private FirebaseClient client = null;

        private static bool initialized = false;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7F61B228-2FE9-4118-A8AF-BD6A44386070");
    }
}