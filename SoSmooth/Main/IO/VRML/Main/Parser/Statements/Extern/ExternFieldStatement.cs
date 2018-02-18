﻿namespace SoSmooth.IO.Vrml.Parser.Statements.Extern
{
    public class ExternFieldStatement
    {
        public string FieldType { get; set; }
        public string FieldId { get; set; }

        public static ExternFieldStatement Parse(ParserContext context)
        {
            context.ReadKeyword("field");

            var fieldType = context.ParseFieldType();
            var fieldId = context.ParseFieldId();

            return new ExternFieldStatement
            {
                FieldType = fieldType,
                FieldId = fieldId,
            };
        }
    }
}
