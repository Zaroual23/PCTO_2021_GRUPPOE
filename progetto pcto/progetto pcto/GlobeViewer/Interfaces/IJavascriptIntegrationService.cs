using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace GlobeViewer.Interfaces
{
    public delegate void MarkerClickedEventHandler(Object sender, string location);

    public class JavascriptEventHandler
    {
        /// <summary>
        /// Raised when a Javascript event arrives (we're using an Action here for brevity).
        /// </summary>
        public event Action<string, dynamic> EventArrived;

        /// <summary>
        /// This method will be exposed to the Javascript environment. It is
        /// invoked in the Javascript environment when some event of interest
        /// happens.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="eventData">Data provided by the invoker pertaining to the event.</param>
        /// <remarks>
        /// By default RaiseEvent will be translated to raiseEvent as a javascript function.
        /// This is configurable when calling RegisterJsObject by setting camelCaseJavascriptNames;
        /// </remarks>
        public void RaiseEvent(string eventName, dynamic eventData)
        {
            EventArrived?.Invoke(eventName, eventData);
        }
    }

    interface IJavascriptIntegrationService
    {
        event MarkerClickedEventHandler MarkerClicked;
        void CallProcedure(string expression);
        JavascriptResponse CallFunction(string expression);
    }
}
