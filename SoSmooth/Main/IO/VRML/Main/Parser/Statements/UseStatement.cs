﻿namespace SoSmooth.Vrml.Parser.Statements
{
    public class UseStatement
    {
        public string NodeName { get; set; }

        public static UseStatement Parse(ParserContext context)
        {
            var keyword = context.ReadNextToken();
            if (keyword.Text != "USE")
            {
                throw new InvalidVrmlSyntaxException("USE expected");
            }

            var nodeName = context.ParseNodeNameId();

            return new UseStatement
            {
                NodeName = nodeName
            };
        }
    }
}
