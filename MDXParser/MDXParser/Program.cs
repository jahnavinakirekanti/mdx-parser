using Antlr4.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDXParser
{
    class Program
    {
        static void Main(string[] args)
        {

            //string inputString = @"SELECT  {[Measures].[Store Sales].[Dept] } ON COLUMNS,
            //   { [Date].[2002], [Date].[2003],[Date].[2008] }  ON ROWS
            //FROM Sales
            //WHERE ( [Store].[USA].[CA] )";
            string inputString = @"WITH MEMBER[measures].[internet profit] AS
[measures].[internet sales amount] - [measures].[internet total product cost],
MEMBER[measures].[anticipated profit] AS
([measures].[internet sales amount] * 1.15 - [measures].[internet total product cost] * 1.05),
SELECT {[measures].[internet profit],
[measures].[anticipated profit]
    }
    ON COLUMNS,
{[product].[category].[children]
}
ON ROWS
FROM[adventure works] WHERE ( [Store].[USA].[CA] )";
            AntlrInputStream input = new AntlrInputStream(inputString);
            Lexer lexer = new mdxLexer(input);

            CommonTokenStream ct = new CommonTokenStream(lexer);
            mdxParser parse = new mdxParser(ct);

            try
            {
                var cst = parse.mdx_statement();
                var ast = new BuildAstVisitor().VisitMdx_statement(cst);
              
                string json = JsonConvert.SerializeObject(ast);
                Console.WriteLine(json);
                Console.ReadLine();
            }
            catch (Exception Ex)
            {
                string name = Ex.Message;
            }
    }
    }

    class MDXObject
    {
        public string CubeName { get; set; }
        
    }
}
