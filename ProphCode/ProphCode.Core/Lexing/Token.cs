using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Lexing
{
    public sealed class Token
    {

        public TokenKind Kind { get; private set; }
        public string Lexeme { get; private set; }
        public int Line { get; private set; }
        public int Col { get; private set; }

        public Token(TokenKind kind, string lexeme, int line, int col)
        {
            Kind = kind; Lexeme = lexeme; Line = line; Col = col;
        }

        public override string ToString() => $"{Kind} \"{Lexeme}\" ({Line},{Col})";
    }
}

