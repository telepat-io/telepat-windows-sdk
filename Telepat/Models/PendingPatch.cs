using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Created by Dorin Damaschin on 22.01.2016
// JSON Patch container

namespace TelepatSDK.Models
{
    public class PendingPatch
    {
        /// <summary>
        /// The operation type. May be add, replace, remove, increment
        /// </summary>
        private PatchType op;

        /// <summary>
        /// The path the operation modifies
        /// </summary>
        private string path;

        /// <summary>
        /// The value the operation modifies
        /// </summary>
        private object value;

        /// <summary>
        /// The ID of the Telepat object
        /// </summary>
        private string objectId;

        /// <summary>
        /// The possible types of patches sent by the SDK
        /// </summary>
        public enum PatchType
        {
            Add,
            Replace,
            Increment,
            Delete
        }

        public PendingPatch(PatchType op, string path, object value, string objectId)
        {
            this.op = op;
            this.path = path;
            this.value = value;
            this.objectId = objectId;
        }

        public PatchType GetOp()
        {
            return op;
        }

        public void SetOp(PatchType op)
        {
            this.op = op;
        }

        public string GetPath()
        {
            return path;
        }

        public void setPath(string path)
        {
            this.path = path;
        }

        public object GetValue()
        {
            return value;
        }

        public void SetValue(string value)
        {
            this.value = value;
        }

        public string getObjectId()
        {
            return objectId;
        }

        public void setObjectId(string objectId)
        {
            this.objectId = objectId;
        }

        /// <summary>
        /// Transforms the PendingPatch into a Map representation that can be sent via the network
        /// </summary>
        /// <returns> Map instance </returns>
        public Dictionary<string, object> ToMap()
        {
            var patch = new Dictionary<string, object>();
            patch.Add("op", op.toString());
            patch.Add("path", path);
            patch.Add("value", value);
            return patch;
        }
    }

    public static class ErrorLevelExtensions
    {
        public static void set(this PendingPatch.PatchType me, string newValue)
        {
            if (newValue == "add") me = PendingPatch.PatchType.Add;
            if (newValue == "replace") me = PendingPatch.PatchType.Replace;
            if (newValue == "increment") me = PendingPatch.PatchType.Increment;
            if (newValue == "delete") me = PendingPatch.PatchType.Delete;
        }

        public static bool EqualsName(this PendingPatch.PatchType me, string otherName)
        {
            return (otherName != null) && me.toString().Equals(otherName);
        }

        public static string toString(this PendingPatch.PatchType me)
        {
            switch (me)
            {
                case PendingPatch.PatchType.Add:
                    return "add";
                case PendingPatch.PatchType.Replace:
                    return "replace";
                case PendingPatch.PatchType.Increment:
                    return "increment";
                case PendingPatch.PatchType.Delete:
                    return "delete";
            }
            return null;
        }
    }
}

