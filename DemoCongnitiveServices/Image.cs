using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;

namespace DemoCongnitiveServices
{
    public class Image
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the faces.
        /// </summary>
        /// <value>
        /// The faces.
        /// </value>
        public IList<Face> Faces { get; set; }
    }
}