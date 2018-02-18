﻿namespace SoSmooth.IO.Vrml.Fields
{
    public class MFRotation : MField<SFRotation>
    {
        public override FieldType Type => FieldType.MFRotation;

        public MFRotation() { }
        public MFRotation(params SFRotation[] items) : base(items) { }

        public override void AcceptVisitor(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override Field Clone()
        {
            var clone = new MFRotation();
            foreach (var child in Values)
            {
                clone.AppendValue((SFRotation)child.Clone());
            }
            return clone;
        }
    }
}
