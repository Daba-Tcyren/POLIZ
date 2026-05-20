using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Scanner
{
    public class SyntError
    {
        public string invalidFragment { get; set; }
        public string location { get; set; }
        public string description { get; set; }

        public SyntError(string invalidFragment, string location, string description)
        {
            this.invalidFragment = invalidFragment;
            this.location = location;
            this.description = description;
        }
    }

    internal class Parser
    {
        private List<SyntError> errors = new List<SyntError>();
        private List<Token> tokens;
        private int currentPos;
        private Token currentToken;
        public List<SyntError> Parse(List<Token> tokens)
        {
          //  this.tokens = MergeErrorTokensSimple(tokens);
            this.tokens = tokens;
            this.errors = new List<SyntError>();

            if (tokens == null || tokens.Count == 0)
            {
                errors.Add(new SyntError("", "", "Пустая строка. Введите строку на распознование"));
                return errors;
            }

            currentToken = this.tokens[currentPos];

            E();

            while (currentPos < tokens.Count) { 
                if(currentToken.id == -1) AddError(currentToken, "Лексическая ошибка", false);
                else AddError(currentToken, "Лишние символы", false);
                GetNextToken();
            }

            if (errors.Count == 0) 
            {
                errors.Add(new SyntError("Успешно", "", "Синтаксический анализ завершен без ошибок"));
            }
            return errors;
        }
        
        // E -> TA

        private void E()
        {
            T();
            A();
        }
        
        // T -> FB

        private void T()
        {
            F();
            B();
        }

        // A -> epsilon | + TA | - TA

        private void A()
        {
            // Выход по пустой цепочке
            if (currentToken == null) return;
            
            // Знаки операции
            else if (currentToken.id == 3 || currentToken.id == 4)
            {
                GetNextToken();
                T();
                A();
            }
        }

        // B -> epsilon | * FB | / FB | % FB

        private void B()
        {
            // Выход по пустой цепочке
            if (currentToken == null) return;

            // Знаки операции
            else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7)
            {
                GetNextToken();
                F();
                B();
            }

            // встретили аргумент
            else if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8)
            {
                AddError(currentToken, "знак оператора");
                GetNextToken();
                if (currentToken == null) return;
                else if (currentToken.id == 3 || currentToken.id == 4) A();
                else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7) B();
                else if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8) T();

            }

            // встретили закрывающую скобку, но нет открывающей
            else if (currentToken.id == 9 && countParenthesis == 0)
            {
                string bufferName = "";
                string bufferLocation = currentToken.location;
                // проходим по циклу до допустимых символов
                while (currentToken != null)
                {
                    bufferName += currentToken.name;
                    // выход
                    if (currentToken.id != 9)
                    {
                        bufferName = bufferName.Replace(currentToken.name, "");
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лишняя скобка", false);
                        B();
                        break;
                    }
                    GetNextToken();
                    // закончились лексемы
                    if (currentToken == null)
                    {
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лишняя скобка", false);
                        return;
                    }
                }
            }

            // недопустимый символ
            else if (currentToken.id == -1)
            {
                string bufferName = "";
                string bufferLocation = currentToken.location;

                // проходим по циклу до допустимых символов
                while (currentToken != null)
                {
                    bufferName += currentToken.name;
                    // выход
                    if (currentToken.id != -1)
                    {
                        bufferName = bufferName.Replace(currentToken.name, "");
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лексическая ошибка", false);
                        break;
                    }
                    GetNextToken();
                    // закончились лексемы
                    if (currentToken == null)
                    {
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лексическая ошибка", false);
                        return;
                    }
                }
                if (currentToken.id == 3 || currentToken.id == 4) A();
                else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7) B();
                else if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8)
                {
                    AddError(currentToken, "знак оператора");
                    T();
                }
            }
            
        }

        // F -> num | id | ( E )
        private int countParenthesis = 0; // кол-во пар скобок

        private void F()
        {
            // встретили закрывающую скобку или конец строки
            if (currentToken == null || currentToken.id == 9)
            {
                AddError(currentToken, "число, индентификатор или открывающая скобка (");
                if (currentToken == null) return;
                string bufferName = "";
                string bufferLocation = currentToken.location;
                // проходим по циклу до допустимых символов
                while (currentToken != null)
                {
                    bufferName += currentToken.name;
                    // выход
                    if (currentToken.id != 9)
                    {
                        bufferName = bufferName.Replace(currentToken.name, "");
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лишняя скобка", false);
                        B();
                        break;
                    }
                    GetNextToken();
                    // закончились лексемы
                    if (currentToken == null)
                    {
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, "Лишняя скобка", false);
                        return;
                    }
                }
                if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8) T();
                else if (currentToken.id == 3 || currentToken.id == 4) A();
                else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7) B();
                else if (currentToken.id == 9) return;
            }

            // встретили идентификатор или число
            else if (currentToken.id == 1 || currentToken.id == 2) 
            {
                GetNextToken();
                return;
            }

            // встретили открывающую скобку
            else if (currentToken.id == 8)
            {
                countParenthesis++;
                GetNextToken();
                E();
                
                if (currentToken == null)
                {
                    AddError(currentToken, "закрывающая скобка )");
                    return;
                }
                else if(currentToken.id == 9)
                {
                    countParenthesis--;
                    GetNextToken();
                    if ( currentToken == null || currentToken.id != 9 ) return;
                    if (countParenthesis > 0) return;
                    string bufferName = "";
                    string bufferLocation = currentToken.location;

                    while (currentToken != null)
                    {
                        bufferName += currentToken.name;
                        if (currentToken.id != 9) 
                        {
                            bufferName = bufferName.Replace(currentToken.name, "");
                            Token lastToken = tokens[currentPos - 1];
                            if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                            {
                                bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                            }
                            else { bufferLocation += " \n" + lastToken.location; }
                            Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                            AddError(errorToken, "Лишняя скобка", false);
                            B();
                            return;
                        }
                        GetNextToken();
                        if (currentToken == null)
                        {
                            Token lastToken = tokens[currentPos - 1];
                            if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                            {
                                bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                            }
                            else { bufferLocation += " \n" + lastToken.location; }
                            Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                            AddError(errorToken, "Лишняя скобка", false);
                            return;
                        }
                    }
                }
                if(currentToken.id == -1)
                {
                    while(currentToken.id == -1)
                    {
                        AddError(currentToken, "Лексическая ошибка", false);
                        GetNextToken();
                        if (currentToken == null) break;
                    }
                    
                }
                if (currentToken == null) { }
                else if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8) T();
                else if (currentToken.id == 3 || currentToken.id == 4) A();
                else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7) B();
                else if (currentToken.id == 9 ) return;

                AddError(currentToken, "закрывающая скобка )");
                GetNextToken();
                return;
            }

            // встретили недопустимый символ
            else if (currentToken.id == -1)
            {
                string bufferName1 = currentToken.name;
                string bufferLocation1 = currentToken.location;

                while (currentToken != null)
                {
                    GetNextToken();
                    if (currentToken == null)
                    {
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation1.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation1 = bufferLocation1.Replace('-' + bufferLocation1.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation1 += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName1, bufferLocation1);
                        AddError(errorToken, "Лексическая ошибка", false);

                        AddError(currentToken, "число, индентификатор или открывающая скобка (");
                        return;
                    }
                    bufferName1 += currentToken.name;
                    if (currentToken.id != -1)
                    {
                        bufferName1 = bufferName1.Replace(currentToken.name, "");
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation1.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation1 = bufferLocation1.Replace('-' + bufferLocation1.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation1 += " \n" + lastToken.location; }
                        Token errorToken = new Token(-1, "", bufferName1, bufferLocation1);
                        AddError(errorToken, "Лексическая ошибка", false);
                        break;
                    }
                }
                if (currentToken.id == 3 || currentToken.id == 4)
                {
                    AddError(currentToken, "число, индентификатор или открывающая скобка (");
                    A();
                }
                else if (currentToken.id == 5 || currentToken.id == 6 || currentToken.id == 7)
                {
                    AddError(currentToken, "число, индентификатор или открывающая скобка (");
                    B();
                }
                else if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8)
                {
                    T();
                }
                return;
            }

            // Проходимся до идентификатора итд
            else
            {
                string bufferName = "";
                string bufferLocation = currentToken.location;

                while (currentToken != null)
                {
                    if(currentToken.id == -1)
                    {
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, " число, индентификатор или открывающая скобка (");


                        return;
                    }
                    bufferName += currentToken.name;
                    if (currentToken.id == 1 || currentToken.id == 2 || currentToken.id == 8 || currentToken.id == 9)
                    {
                        bufferName = bufferName.Replace(currentToken.name, "");
                        Token lastToken = tokens[currentPos - 1];
                        if (bufferLocation.Split()[1] == lastToken.location.Split()[1])
                        {
                            bufferLocation = bufferLocation.Replace('-' + bufferLocation.Split()[2].Split('-')[1], '-' + lastToken.location.Split()[2].Split('-')[1]);
                        }
                        else { bufferLocation += " \n" + lastToken.location; }

                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, " число, индентификатор или открывающая скобка (");
                        F();
                        return;
                    }
                    GetNextToken();
                    if (currentToken == null)
                    {
                        Token errorToken = new Token(-1, "", bufferName, bufferLocation);
                        AddError(errorToken, " число, индентификатор или открывающая скобка (");
                        return;
                    }
                }
            }

        }

        private void AddError(Token token, string expected, bool desc = true)
        {
            string invalidFragment = token?.name ?? "Конец строки";
            string location = token?.location ?? "позиция неизвестна";
            string description = $"Ожидалось {expected}. Встречено '{invalidFragment}'";
            if(desc == false)
            {
                errors.Add(new SyntError(invalidFragment, location, expected));
                return;
            }
            errors.Add(new SyntError(invalidFragment, location, description));
        }
        
        private void GetNextToken()
        {
            currentPos++;
            if (currentPos < tokens.Count)
            {
                currentToken = tokens[currentPos];
            }
            else
            {
                currentToken = null;
            }
        }
    }
}
