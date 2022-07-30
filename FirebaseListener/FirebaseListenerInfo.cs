﻿using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace FirebaseListener
{
    public class FirebaseListenerInfo : GH_AssemblyInfo
    {
        public override string Name => "FirebaseListener";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("7EE28882-C741-4A10-B007-3797C67D8040");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}