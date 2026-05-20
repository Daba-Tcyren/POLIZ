using System;
using System.Collections.Generic;
#pragma warning disable IDE0055

namespace Scanner
{
    public class Token
    {
        public readonly int id;
        public readonly string type;
        public readonly string name;
        public readonly string location;
        public Token(int id, string type, string name, string location)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.location = location;
        }
    }
    public class scanner
    {
        private string text;
        private char liter;
        private int currentPosition = 0;
        private int positionLine = 0;
        private int currentLine = 1;
        private string buffer = "";
        private List<Token> tokens = new List<Token>();
        public List<Token> analyze(string inputText)
        {
            text = inputText;

            getNext();

            while (currentPosition <= text.Length)
            {
                if (IsLetter(liter))
                {
                    buffer += liter;
                    while (IsLetter(liter = getChar()) || char.IsDigit(liter) || liter == '_')
                    {
                        buffer += liter;
                    }
                    addToken(1, "Идентификатор", buffer);
                    buffer = "";
                }
                else if (char.IsDigit(liter))
                {
                    buffer += liter;
                    while (char.IsDigit(liter = getChar()))
                    {
                        buffer += liter;
                    }
                    addToken(2, "Целое число без знака", buffer);
                    buffer = "";
                }
                else
                {
                    switch (liter)
                    {
                        case '\0':
                            getNext();
                            break;
                        case '\n':
                            positionLine = 0;
                            currentLine++;
                            getNext();
                            break;
                        case ' ':
                            getNext();
                            break;
                        case '-':
                            buffer += liter;
                            getNext();
                            addToken(3, "Знак вычитания", buffer);
                            buffer = "";
                            break;
                        case '+':
                            buffer += liter;
                            getNext();
                            addToken(4, "Знак сложения", buffer);
                            buffer = "";
                            break;
                        case '*':
                            buffer += liter;
                            getNext();
                            addToken(5, "Знак умножения", buffer);
                            buffer = "";
                            break;
                        case '/':
                            buffer += liter;
                            getNext();
                            addToken(6, "Знак деления", buffer);
                            buffer = "";
                            break;
                        case '%':
                            buffer += liter;
                            getNext();
                            addToken(7, "Знак остаток от деления", buffer);
                            buffer = "";
                            break;
                        case '(':
                            buffer += liter;
                            getNext();
                            addToken(8, "Открывающая скобка", buffer);
                            buffer = "";
                            break;
                        case ')':
                            buffer += liter;
                            getNext();
                            addToken(9, "Закрывающая скобка", buffer);
                            buffer = "";
                            break;
                        default:
                            buffer += liter;
                            getNext();
                            addToken(-1, "Недопустимый символ", buffer);
                            buffer = "";
                            break;
                    }
                }
            }

            return tokens;
        }
        private char getChar()
        {
            try
            {
                if (currentPosition >= text.Length)
                {
                    currentPosition++;
                    positionLine++;
                    return '\0';
                }
                char liter1 = text[currentPosition];
                currentPosition++;
                positionLine++;
                return liter1;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception("В конце строки не обнаружено ;");
            }
        }
        
        private void getNext()
        {
            liter = getChar();
        }
        private void addToken(int id, string type, string name)
        {
            int Length = name.Length;
            int leng = positionLine - Length;
            string loc = $"строка {currentLine}, {leng}-{positionLine - 1}";
            tokens.Add(new Token(id, type, name, loc));
        }
        private bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }
    }
}
