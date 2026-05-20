using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Antlr4.Runtime.Atn.SemanticContext;
#pragma warning disable IDE0055
namespace Scanner
{
    public class Tetrad
    {
        public string operation { get; set; }
        public string argument1 { get; set; }
        public string argument2 { get; set; }
        public string result { get; set; }

        public Tetrad(string operation, string argument1, string argument2, string result)
        {
            this.operation = operation;
            this.argument1 = argument1;
            this.argument2 = argument2;
            this.result = result;
        }
    }
    internal class POLIZ
    {
        List<Tetrad> tetrads = new List<Tetrad>();
        private List<Token> tokens;
        private string postfixNotation = "";
        private Stack<Token> operationsStack = new Stack<Token>();
        private bool result = true;

        public List<Tetrad> calculate(List<Token> Tokens)
        {
            this.tokens = Tokens;
            List<Token> tetradNotaion = new List<Token>();

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].id == 1 || tokens[i].id == 2)
                {
                    if (tokens[i].id == 1) result = false;
                    
                    postfixNotation += tokens[i].name + " ";
                    tetradNotaion.Add(tokens[i]);
                }
                else
                {
                    if (operationsStack.Count == 0)
                    {
                        operationsStack.Push(tokens[i]);
                        continue;
                    }

                    Token lastOperationToken = operationsStack.Pop();
                    char lastOperation = lastOperationToken.name[0];
                    char Operation = tokens[i].name[0];
                    if (OperationPriority(Operation) > OperationPriority(lastOperation) || OperationPriority(Operation) == 0)
                    {
                        operationsStack.Push(lastOperationToken);
                        operationsStack.Push(tokens[i]);
                    }
                    else if(tokens[i].id == 9)
                    {
                        while (lastOperation != '(')
                        {
                            tetrads.Add(new Tetrad(lastOperationToken.name, tetradNotaion[tetradNotaion.Count - 2].name, tetradNotaion[tetradNotaion.Count - 1].name, "T" + (tetrads.Count() + 1).ToString()));
                            tetradNotaion.RemoveRange(tetradNotaion.Count - 2, 2);
                            tetradNotaion.Insert(tetradNotaion.Count, new Token(-1, "", "T" + tetrads.Count().ToString(), ""));

                            postfixNotation += lastOperationToken.name + " ";

                            lastOperationToken = operationsStack.Pop();
                            lastOperation = lastOperationToken.name[0];
                        }
                    }
                    else
                    {
                        tetrads.Add(new Tetrad(lastOperationToken.name, tetradNotaion[tetradNotaion.Count - 2].name, tetradNotaion[tetradNotaion.Count - 1].name, "T" + (tetrads.Count() + 1).ToString()));
                        tetradNotaion.RemoveRange(tetradNotaion.Count - 2, 2);
                        tetradNotaion.Insert(tetradNotaion.Count, new Token(-1, "", "T" + tetrads.Count().ToString(), ""));

                        postfixNotation += lastOperationToken.name + " ";

                        if (operationsStack.Count == 0)
                        {
                            operationsStack.Push(tokens[i]);
                            continue;
                        }

                        lastOperationToken = operationsStack.Pop();
                        lastOperation = lastOperationToken.name[0];

                        if (OperationPriority(Operation) > OperationPriority(lastOperation) || OperationPriority(Operation) == 0)
                        {
                            operationsStack.Push(lastOperationToken);
                            operationsStack.Push(tokens[i]);
                        }
                        else
                        {
                            tetrads.Add(new Tetrad(lastOperationToken.name, tetradNotaion[tetradNotaion.Count - 2].name, tetradNotaion[tetradNotaion.Count - 1].name, "T" + (tetrads.Count() + 1).ToString()));
                            tetradNotaion.RemoveRange(tetradNotaion.Count - 2, 2);
                            tetradNotaion.Insert(tetradNotaion.Count, new Token(-1, "", "T" + tetrads.Count().ToString(), ""));

                            postfixNotation += lastOperationToken.name + " ";
                            operationsStack.Push(tokens[i]);
                        }
                    }
                }
            }
            while (operationsStack.Count > 0)
            {
                string operation = operationsStack.Pop().name;
                tetrads.Add(new Tetrad(operation, tetradNotaion[tetradNotaion.Count - 2].name, tetradNotaion[tetradNotaion.Count - 1].name, "T" + (tetrads.Count() + 1).ToString()));
                tetradNotaion.RemoveRange(tetradNotaion.Count - 2, 2);
                tetradNotaion.Insert(tetradNotaion.Count, new Token(-1, "", "T" + tetrads.Count().ToString(), ""));

                postfixNotation += operation + " ";
            }
            return tetrads;
        }
        private int OperationPriority(char operation)
        {
            switch (operation)
            {
                case '(':
                    return 0;
                case ')':
                    return 1;
                case '-':
                case '+':
                    return 7;
                case '%':
                case '/':
                case '*':
                    return 8;
            }
            return -1;
        }
        public string getResultPOLIZ()
        {
            string output = "\n\n\nРезультат: ";

            Dictionary<string, int> tetradResult = new Dictionary<string, int>();
            int x, y, resultOperation = 0;

            if (result)
            {
                foreach(Tetrad tetrad in tetrads)
                {
                    if(!int.TryParse(tetrad.argument1,out x))
                    {
                        tetradResult.TryGetValue(tetrad.argument1, out x);
                    }
                    if (!int.TryParse(tetrad.argument2, out y))
                    {
                        tetradResult.TryGetValue(tetrad.argument2, out y);
                    }
                    switch (tetrad.operation[0])
                    {
                        case '-':
                            resultOperation = x - y;
                            break;
                        case '+':
                            resultOperation = x + y;
                            break;
                        case '%':
                            resultOperation = x % y;
                            break;
                        case '/':
                            resultOperation = x / y;
                            break;
                        case '*':
                            resultOperation = x * y;
                            break;
                    }
                    tetradResult.Add(tetrad.result, resultOperation);
                }
                return postfixNotation + output + resultOperation.ToString();
            }
            return postfixNotation + output + "Расчеты невозможны. В выражении имеются переменные.";
        }
    }
}
