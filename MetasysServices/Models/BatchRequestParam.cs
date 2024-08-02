using System;
using System.Collections.Generic;
using System.Text;

namespace JohnsonControls.Metasys.BasicServices
{
    /// <summary>
    /// Body class to pass list of parameters for Metasys server batch request.
    /// </summary>

    public class BatchRequestParam
    {
        /// <summary> Id to identify the object </summary>
        public ObjectId ObjectId { get; set; }

        /// <summary> String that could represent a text resource as, for instance, a text annotation </summary>
        public string Resource { get; set; }

        /// <summary> Activity Type. The possible values are: 'alarm', 'audit' </summary>
        public string ActivityType { get; set; } = "";

        /// <summary> Activity Management Status. The possible values are: 'discarded' (for Alarms and Audits), 'acknowledged' (for Audits) </summary>
        public string ActivityManagementStatus { get; set; } = "";

    }
}
