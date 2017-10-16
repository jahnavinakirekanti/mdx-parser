using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

namespace MDXParser
{

    class BuildAST
    {
    }

    class ASTNode
    {
    }
    internal abstract class ExpressionNode
    {
    }
    internal class NumberNode : ExpressionNode
    {
        public string Value { get; set; }
    }


    internal class SelectNode : ExpressionNode
    {
        public ExpressionNode AxisNode { get; set; }
        public ExpressionNode CubeNode { get; set; }
        public ExpressionNode WhereNode { get; set; }
        public ExpressionNode formulaNode { get; set; }
    }
    internal class CubeNode : ExpressionNode
    {
        public string CubeName { get; set; }
    }
    internal class AxisListNode : ExpressionNode
    {
        public List<ExpressionNode> Axis { get; set; }
    }
    internal class MemberListNode : ExpressionNode
    {
        public List<ExpressionNode> Members { get; set; }
    }


    internal class AxisNode : ExpressionNode
    {
        public string AxisName { get; set; }
        public ExpressionNode Dimensions { get; set; }
    }
    internal class MembersNode : ExpressionNode
    {
        public string MemberName { get; set; }
        public ExpressionNode Dimensions { get; set; }
    }
    internal class DimensionNodes : ExpressionNode
    {
        public List<ExpressionNode> List { get; set; }
    }
    internal class DimensionNode : ExpressionNode
    {
        public ExpressionNode Measures { get; set; }
        public ExpressionNode MeasureValue { get; set; }
    }
    internal class FormulaListNode : ExpressionNode
    {
        public List<ExpressionNode> Formula { get; set; }
    }
    internal class Operator : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
        public string Operation { get; set; }
    }

    //internal class Minus: Multiple
    //{

    //}
    //internal class Plus : Multiple
    //{

    //}
    internal class BuildAstVisitor : mdxBaseVisitor<ExpressionNode>
    {
        public override ExpressionNode VisitMdx_statement(mdxParser.Mdx_statementContext context)
        {
            return Visit(context.select_statement());
        }

        public override ExpressionNode VisitSet_specification(mdxParser.Set_specificationContext context)
        {
            return new NumberNode
            {
                Value = ""
            };
        }
        public override ExpressionNode VisitSelect_statement([NotNull] mdxParser.Select_statementContext context)
        {
            SelectNode selectNode = new SelectNode();
            selectNode.CubeNode = Visit(context.cube_specification());
            selectNode.AxisNode = Visit(context.axis_specification_list());
            selectNode.formulaNode = Visit(context.formula_specification());
            DimensionNodes dim = (DimensionNodes)Visit(context.slicer_specification());
            selectNode.WhereNode = dim;

            return selectNode;
        }

        public override ExpressionNode VisitSlicer_specification([NotNull] mdxParser.Slicer_specificationContext context)
        {
            return base.VisitSlicer_specification(context);
        }
        public override ExpressionNode VisitCube_specification([NotNull] mdxParser.Cube_specificationContext context)
        {
            return new CubeNode
            {
                CubeName = context.Start.Text
            };
        }
        public override ExpressionNode VisitCube_name([NotNull] mdxParser.Cube_nameContext context)
        {
            return Visit(context.compound_id());

        }
        public override ExpressionNode VisitCompound_id([NotNull] mdxParser.Compound_idContext context)
        {

            if (context.children.Count > 1)
            {
                var List = context.children;
                var Dimension = new NumberNode();
                Dimension.Value = "";
                for (int i = 0; i < List.Count; i = i + 2)
                {
                    NumberNode node = (NumberNode)Visit(List[i]);
                    Dimension.Value += node.Value;
                }


                return Dimension;
            }
            else
            {
                return new CubeNode
                {
                    CubeName = context.Start.Text
                };
            }




        }

        public override ExpressionNode VisitAxis_specification_list([NotNull] mdxParser.Axis_specification_listContext context)
        {
            AxisListNode AxisList = new AxisListNode();
            AxisList.Axis = new List<ExpressionNode>();
            var List = context.axis_specification().ToList();

            foreach (var axis in List)
            {
                AxisNode Axis = new AxisNode();
                Axis.AxisName = axis.axis_name().Start.Text;
                Axis.Dimensions = Visit(axis.expression());
                AxisList.Axis.Add(Axis);
            }
            return AxisList;
        }

        public override ExpressionNode VisitAxis_specification([NotNull] mdxParser.Axis_specificationContext context)
        {


            return Visit(context.expression());
        }
        public override ExpressionNode VisitExpression([NotNull] mdxParser.ExpressionContext context)
        {


            var k = base.VisitExpression(context);
            return k;

        }
        public override ExpressionNode VisitExp_list([NotNull] mdxParser.Exp_listContext context)
        {
            if (context.children.Count > 1)
            {
                var k = context.children;

                DimensionNodes DimensionList = new DimensionNodes();
                DimensionList.List = new List<ExpressionNode>();
                DimensionList.List.Add(Visit(k[0]));

                for (int i = 2; i < k.Count; i = i + 2)
                {
                    DimensionList.List.Add(Visit(k[i]));
                }

                return DimensionList;
            }
            else
            {
                DimensionNodes DimensionList = new DimensionNodes();
                DimensionList.List = new List<ExpressionNode>();
                DimensionList.List.Add(base.VisitExp_list(context));

                return DimensionList;
            }
        }
        public override ExpressionNode VisitTerm([NotNull] mdxParser.TermContext context)
        {

            if (context.children.Count > 1)
            {
                var List = context.children;
                DimensionNodes DimensionList = new DimensionNodes();
                DimensionList.List = new List<ExpressionNode>();

                for (int i = 1; i < List.Count; i = i + 2)
                {


                    Operator mul = new Operator();
                    mul.Left = Visit(List[i - 1]);
                    mul.Right = Visit(List[i + 1]);
                    var k = List[i].Payload;
                    mul.Operation = (string)k.GetType().GetProperty("Text").GetValue(k, null);
                    DimensionList.List.Add(mul);
                }
                return DimensionList;
            }
            else
            {
                var k = base.VisitTerm(context);
                return k;
            }

        }
        public override ExpressionNode VisitTerm2([NotNull] mdxParser.Term2Context context)
        {

            if (context.children.Count > 1)
            {
                var List = context.children;
                DimensionNodes DimensionList = new DimensionNodes();
                DimensionList.List = new List<ExpressionNode>();

                for (int i = 0; i < List.Count; i = i + 2)
                {
                    DimensionList.List.Add(Visit(List[i]));
                }
                return DimensionList;
            }
            else
            {
                var k = base.VisitTerm2(context);
                return k;
            }


        }
        public override ExpressionNode VisitTerm3([NotNull] mdxParser.Term3Context context)
        {
            var k = base.VisitTerm3(context);
            return k;
        }
        public override ExpressionNode VisitTerm4([NotNull] mdxParser.Term4Context context)
        {
            var k = base.VisitTerm4(context);
            return k;
        }
        public override ExpressionNode VisitTerm5([NotNull] mdxParser.Term5Context context)
        {
            var k = base.VisitTerm5(context);
            return k;
        }
        public override ExpressionNode VisitValue_expression([NotNull] mdxParser.Value_expressionContext context)
        {

            var k = base.VisitValue_expression(context);
            return k;


        }
        public override ExpressionNode VisitFactor([NotNull] mdxParser.FactorContext context)
        {
            if (context.children.Count > 1)
            {
                var List = context.children;
                DimensionNodes DimensionList = new DimensionNodes();
                DimensionList.List = new List<ExpressionNode>();

                for (int i = 1; i < List.Count; i = i + 2)
                {

                    Operator mul = new Operator();
                    var k = List[i - 1].Payload;

                    mul.Left = new NumberNode
                    {
                        Value = (string)k.GetType().GetProperty("Text").GetValue(k, null)
                    };
                    mul.Right = Visit(List[i + 1]);
                    k = List[i].Payload;
                    mul.Operation = (string)k.GetType().GetProperty("Text").GetValue(k, null);

                    DimensionList.List.Add(mul);
                }
                return DimensionList;
            }
            else
            {
                var k = base.VisitFactor(context);
                return k;
            }

        }
        public override ExpressionNode VisitValue_expression_primary([NotNull] mdxParser.Value_expression_primaryContext context)
        {

            if (context.children.Count > 1)
            {
                var k = context.children;
                var Dimensions = new DimensionNode();

                Dimensions.Measures = Visit(k[0]);
                NumberNode Values = new NumberNode();
                Values.Value = "";
                for (int i = 2; i < k.Count; i = i + 2)
                {
                    NumberNode TempNode = (NumberNode)Visit(k[i]);
                    Values.Value += TempNode.Value + ".";

                }
                Values.Value = Values.Value.Substring(0, Values.Value.Length - 1);
                Dimensions.MeasureValue = Values;
                return Dimensions;
            }
            else
            {
                var k = base.VisitValue_expression_primary(context);
                return k;
            }

        }
        public override ExpressionNode VisitQuoted_identifier([NotNull] mdxParser.Quoted_identifierContext context)
        {

            return new NumberNode
            {
                Value = context.Start.Text
            };
        }
        public override ExpressionNode VisitUnquoted_identifier([NotNull] mdxParser.Unquoted_identifierContext context)
        {
            return base.VisitUnquoted_identifier(context);
        }
        public override ExpressionNode VisitAmp_quoted_identifier([NotNull] mdxParser.Amp_quoted_identifierContext context)
        {
            return base.VisitAmp_quoted_identifier(context);
        }
        public override ExpressionNode VisitFunction([NotNull] mdxParser.FunctionContext context)
        {

            var k = base.VisitFunction(context);
            return k;
        }
        public override ExpressionNode VisitFormula_specification([NotNull] mdxParser.Formula_specificationContext context)
        {
            MemberListNode MemberList = new MemberListNode();
            MemberList.Members = new List<ExpressionNode>();
            var List = context.single_formula_specification().ToArray();

            foreach (var members in List)
            {
                MembersNode Member = new MembersNode();
                var k = Visit(members);
                MemberList.Members.Add(k);
            }
            return MemberList;

        }

        public override ExpressionNode VisitValue_expression_primary0([NotNull] mdxParser.Value_expression_primary0Context context)
        {
            if (context.children.Count > 1)
            {
                var List = context.children;

                var Dimensions = Visit(List[1]);
                return Dimensions;
            }
            else
            {
                var k = base.VisitValue_expression_primary0(context);
                return k;
            }
        }
        public override ExpressionNode VisitChildren(IRuleNode node)
        {
            return base.VisitChildren(node);
        }

        public override ExpressionNode VisitValue_or_expression([NotNull] mdxParser.Value_or_expressionContext context)
        {
            var k = base.VisitValue_or_expression(context);
            return k;

        }
        public override ExpressionNode VisitCase_expression([NotNull] mdxParser.Case_expressionContext context)
        {
            var k = base.VisitCase_expression(context);
            return k;
        }

        public override ExpressionNode VisitValue_xor_expression([NotNull] mdxParser.Value_xor_expressionContext context)
        {
            var k = base.VisitValue_xor_expression(context);
            return k;
        }
        public override ExpressionNode VisitAxis_name([NotNull] mdxParser.Axis_nameContext context)
        {
            var k = base.VisitAxis_name(context);
            return k;
        }

        public override ExpressionNode VisitDim_props([NotNull] mdxParser.Dim_propsContext context)
        {
            var k = Visit(context.property_list());
            return k;
        }
        public override ExpressionNode VisitProperty_list([NotNull] mdxParser.Property_listContext context)
        {
            string ter = context.ToStringTree();
            var prop = context.property();
            return base.VisitProperty_list(context);
        }


        public override ExpressionNode VisitCell_property_list([NotNull] mdxParser.Cell_property_listContext context)
        {
            var k = base.VisitCell_property_list(context);
            return k;
        }

        public override ExpressionNode VisitSingle_formula_specification([NotNull] mdxParser.Single_formula_specificationContext context)
        {
            var k = base.VisitSingle_formula_specification(context);
            return k;
        }
        public override ExpressionNode VisitMember_specification([NotNull] mdxParser.Member_specificationContext context)
        {


            MembersNode node = new MembersNode();

            NumberNode k = (NumberNode)Visit(context.member_name());
            node.MemberName = k.Value;
            node.Dimensions = Visit(context.value_expression());
            return node;
        }
        public override ExpressionNode VisitMember_name([NotNull] mdxParser.Member_nameContext context)
        {
            var k = base.VisitMember_name(context);
            return k;
        }
        public override ExpressionNode VisitIdentifier([NotNull] mdxParser.IdentifierContext context)
        {
            var k = base.VisitIdentifier(context);
            return k;
        }

    }
}



