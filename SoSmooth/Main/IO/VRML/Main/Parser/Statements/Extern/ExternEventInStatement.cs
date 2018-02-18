﻿namespace SoSmooth.IO.Vrml.Parser.Statements.Extern
{
    public class ExternEventInStatement
    {
        public string FieldType { get; set; }
        public string EventId { get; set; }

        public static ExternEventInStatement Parse(ParserContext context)
        {
            context.ReadKeyword("eventIn");

            var fieldType = context.ParseFieldType();
            var eventId = context.ParseEventInId();
            return new ExternEventInStatement
            {
                FieldType = fieldType,
                EventId = eventId
            };
        }
    }
}
