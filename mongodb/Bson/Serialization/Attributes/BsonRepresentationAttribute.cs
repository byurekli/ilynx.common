﻿/* Copyright 2010-2011 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Attributes {
    /// <summary>
    /// Specifies the external representation and related options for this field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonRepresentationAttribute : BsonSerializationOptionsAttribute {
        #region private fields
        private BsonType representation;
        private bool allowOverflow;
        private bool allowTruncation;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the BsonRepresentationAttribute class.
        /// </summary>
        /// <param name="representation">The external representation.</param>
        public BsonRepresentationAttribute(
            BsonType representation
        ) {
            this.representation = representation;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets the external representation.
        /// </summary>
        public BsonType Representation {
            get { return representation; }
        }

        /// <summary>
        /// Gets or sets whether to allow overflow.
        /// </summary>
        public bool AllowOverflow {
            get { return allowOverflow; }
            set { allowOverflow = value; }
        }

        /// <summary>
        /// Gets or sets whether to allow truncation.
        /// </summary>
        public bool AllowTruncation {
            get { return allowTruncation; }
            set { allowTruncation = value; }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Gets the serialization options specified by this attribute.
        /// </summary>
        /// <returns>The serialization options.</returns>
        public override IBsonSerializationOptions GetOptions() {
            return new RepresentationSerializationOptions(representation, allowOverflow, allowTruncation);
        }
        #endregion
    }
}
