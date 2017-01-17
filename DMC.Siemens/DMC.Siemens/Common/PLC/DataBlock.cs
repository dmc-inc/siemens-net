﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMC.Siemens.Common.Base;
using DMC.Siemens.Portal.Base;

namespace DMC.Siemens.Common.PLC
{
    public class DataBlock : DataEntity
    {

        #region Public Properties

        private bool _IsOptimized;
        public bool IsOptimized
        {
            get
            {
                return this._IsOptimized;
            }
            set
            {
                this.SetProperty(ref this._IsOptimized, value);
            }
        }

        private bool _IsInstance;
        public bool IsInstance
        {
            get
            {
                return this._IsInstance;
            }
            set
            {
                this.SetProperty(ref this._IsInstance, value);
            }
        }

        private bool _IsDataType;
        public bool IsDataType
        {
            get
            {
                return this._IsDataType;
            }
            set
            {
                this.SetProperty(ref this._IsDataType, value);
            }
        }

        private string _InstanceName;
        public string InstanceName
        {
            get
            {
                return this._InstanceName;
            }
            set
            {
                this.SetProperty(ref this._InstanceName, value);
            }
        }

        #endregion

        #region Protected Properties

        protected override string DataHeader
        {
            get
            {
                return (this.IsOptimized) ? "VAR" : "STRUCT";
            }
        }

		private IDictionary<DataEntry, IAddress> Addresses { get; } = new Dictionary<DataEntry, IAddress>();

		#endregion

		#region Public Methods

		public override IParsableSource ParseSource(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("S7_Optimized_Access"))
                {
                    this.IsOptimized = line.ToUpper().Contains("TRUE");
                    base.ParseSource(reader);
                    break;
                }
                else if (line.Contains("TITLE ="))
                {
                    this.IsOptimized = false;
                    base.ParseSource(reader);
                    break;
                }
            }

            this.Type = BlockType.DataBlock;

            return this;

        }
		
		public IDictionary<DataEntry, IAddress> CalcluateAbsoluteAddresses()
		{
			foreach (DataEntry entry in this)
			{
				;
			}

			return this.Addresses;
		}

        #endregion
        

    }
}
