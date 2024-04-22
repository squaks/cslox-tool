using System.Text;

namespace cslox_tool
{
    // expression     → equality ;
    // equality       → comparison(( "!=" | "==" ) comparison )* ;
    // comparison     → term(( ">" | ">=" | "<" | "<=" ) term )* ;
    // term           → factor(( "-" | "+" ) factor )* ;
    // factor         → unary(( "/" | "*" ) unary )* ;
    // unary          → ( "!" | "-" ) unary
    //                | primary ;
    // primary        → NUMBER | STRING | "true" | "false" | "nil"
    //                | "(" expression ")" ;
    internal class GenerateAst 
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output directory>");
                Environment.Exit(64);
            }
            string outputDir = args[0];
            defineAst(outputDir, "Expr", new[] {
                "Binary   : Expr left, Token op, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token op, Expr right"
                }
            );
        }

        private static void defineAst(string outputDir, string baseName, string[] types)
        {
            string path = outputDir + "\\" + baseName + ".cs";
            StreamWriter writer = new(path, false, Encoding.UTF8);

            writer.WriteLine("namespace cslox");
            writer.WriteLine("{");
            writer.WriteLine("\tinternal abstract class " + baseName);
            writer.WriteLine("\t{");

            defineVisitor(writer, baseName, types);
                    
            // The AST classes.
            for (int i = 0; i < types.Length; i++)
            {
                string className = types[i].Split(":")[0].Trim();
                string fields    = types[i].Split(":")[1].Trim();
                defineType(writer, baseName, className, fields);
            }

            // The base accept() method
            writer.WriteLine("");
            writer.WriteLine("\t\tpublic abstract R accept<R>(IAstVisitor<R> visitor);");

            writer.WriteLine("\t}");
            writer.WriteLine("}");
            writer.Close();
        }
        private static void defineVisitor(StreamWriter writer, string baseName, string[] types)
        {
            writer.WriteLine("\t\tpublic interface IAstVisitor<R>");
            writer.WriteLine("\t\t{");

            for(int i = 0;i < types.Length;i++)
            {
                string typeName = types[i].Split(":")[0].Trim();
                writer.WriteLine("\t\t\tR visit" + typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");");
            }

            writer.WriteLine("\t\t}");
        }

        private static void defineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine("\t\tpublic class " + className + " : " + baseName);
            writer.WriteLine("\t\t{");

            // Fields.
            string[] fields = fieldList.Split(", ");
            for(int i = 0; i < fields.Length; i++)
            {
                writer.WriteLine("\t\t\tpublic readonly " + fields[i] + ";");
            }
            writer.WriteLine();

            // Constructor.
            writer.WriteLine("\t\t\tpublic " + className + "(" + fieldList + ")");
            writer.WriteLine("\t\t\t{");

            // Store arguments in fields.
            for (int i = 0; i < fields.Length; i++)
            {
                string name = fields[i].Split(" ")[1];
                writer.WriteLine("\t\t\t\tthis." + name + " = " + name + ";");
            }
            writer.WriteLine("\t\t\t}");

            //Visitor Pattern.
            writer.WriteLine();
            writer.WriteLine("\t\t\tpublic override R accept<R>(IAstVisitor<R> visitor)");
            writer.WriteLine("\t\t\t{");
            writer.WriteLine("\t\t\t\treturn visitor.visit" + className + baseName + "(this);");
            writer.WriteLine("\t\t\t}");

            writer.WriteLine("\t\t}");
        }
    }
}
