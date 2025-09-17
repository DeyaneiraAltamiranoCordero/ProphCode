using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Lexing
{
    public sealed class Lexer
    {
        private string _src = "";
        private int _i, _line, _col;
        private List<Token> _tokens= new List<Token>();

        public IList<Token> Tokenize(string source)
        {
            _src = source ?? string.Empty;
            _i = 0; _line = 1; _col = 1;
            _tokens = new List<Token>();

            while (!IsAtEnd())
            {
                SkipWhitespaceAndComments();
                if (IsAtEnd()) break;

                int startLine = _line, startCol = _col;
                char c = Peek();

                if (IsLetter(c) || c == '_') { LexWord(startLine, startCol); continue; }
                if (IsDigit(c)) { LexNumber(startLine, startCol); continue; }
                if (c == '"') { LexString(startLine, startCol); continue; }
                if (c == '\'') { LexChar(startLine, startCol); continue; }

                // Operadores/puntuación (max-munch)
                if (Match("->")) { Add(TokenKind.Arrow, "->", startLine, startCol); continue; }
                if (Match("==")) { Add(TokenKind.EqEq, "==", startLine, startCol); continue; }
                if (Match("!=")) { Add(TokenKind.BangEq, "!=", startLine, startCol); continue; }

                // Un solo carácter
                c = Advance();
                switch (c)
                {
                    case '+': Add(TokenKind.Plus, "+", startLine, startCol); break;
                    case '-': Add(TokenKind.Minus, "-", startLine, startCol); break;
                    case '*': Add(TokenKind.Star, "*", startLine, startCol); break;
                    case '/': Add(TokenKind.Slash, "/", startLine, startCol); break;
                    case '%': Add(TokenKind.Percent, "%", startLine, startCol); break;

                    case '(': Add(TokenKind.LParen, "(", startLine, startCol); break;
                    case ')': Add(TokenKind.RParen, ")", startLine, startCol); break;
                    case '{': Add(TokenKind.LBrace, "{", startLine, startCol); break;
                    case '}': Add(TokenKind.RBrace, "}", startLine, startCol); break;
                    case '[': Add(TokenKind.LBracket, "[", startLine, startCol); break;
                    case ']': Add(TokenKind.RBracket, "]", startLine, startCol); break;
                    case ';': Add(TokenKind.Semi, ";", startLine, startCol); break;
                    case ',': Add(TokenKind.Comma, ",", startLine, startCol); break;
                    case ':': Add(TokenKind.Colon, ":", startLine, startCol); break;

                    default:
                        throw new Exception($"[Lex] Carácter no reconocido '{c}' ({startLine},{startCol})");
                }
            }

            Add(TokenKind.Eof, string.Empty, _line, _col);
            return _tokens;
        }

        // ---------- helpers de palabras, números, strings, char ----------
        private void LexWord(int startLine, int startCol)
        {
            var sb = new StringBuilder();
            sb.Append(Advance()); // primera letra/_

            while (!IsAtEnd() && (IsLetterOrDigit(Peek()) || Peek() == '_'))
                sb.Append(Advance());

            var word = sb.ToString();
            if (Words.Map.TryGetValue(word, out var kind))
            {
                Add(kind, word, startLine, startCol);
            }
            else
            {
                Add(TokenKind.Ident, word, startLine, startCol);
            }
        }

        private void LexNumber(int startLine, int startCol)
        {
            var sb = new StringBuilder();
            while (!IsAtEnd() && IsDigit(Peek())) sb.Append(Advance());

            if (!IsAtEnd() && Peek() == '.' && IsDigit(Peek(1)))
            {
                sb.Append(Advance()); // '.'
                while (!IsAtEnd() && IsDigit(Peek())) sb.Append(Advance());
                Add(TokenKind.DecLit, sb.ToString(), startLine, startCol);
            }
            else
            {
                Add(TokenKind.IntLit, sb.ToString(), startLine, startCol);
            }
        }

        private void LexString(int startLine, int startCol)
        {
            Advance(); // consumir '"'
            var sb = new StringBuilder();
            while (!IsAtEnd() && Peek() != '"')
            {
                char c = Advance();
                if (c == '\\') // escape
                {
                    if (IsAtEnd()) throw new Exception($"[Lex] Escape mal formado ({_line},{_col})");
                    char e = Advance();
                    switch (e)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('\"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: throw new Exception($"[Lex] Escape '\\{e}' no válido ({_line},{_col})");
                    }
                }
                else sb.Append(c);
            }
            if (IsAtEnd()) throw new Exception($"[Lex] Cadena no terminada ({startLine},{startCol})");
            Advance(); // cerrar '"'
            Add(TokenKind.TextLit, sb.ToString(), startLine, startCol);
        }

        private void LexChar(int startLine, int startCol)
        {
            Advance(); // consumir '\''
            char value;
            if (IsAtEnd()) throw new Exception($"[Lex] Char no terminado ({startLine},{startCol})");

            if (Peek() == '\\') // escape
            {
                Advance();
                if (IsAtEnd()) throw new Exception($"[Lex] Char escape mal formado ({_line},{_col})");
                char e = Advance();
                switch (e)
                {
                    case 'n': value = '\n'; break;
                    case 't': value = '\t'; break;
                    case '\\': value = '\\'; break;
                    case '\'': value = '\''; break;
                    default: throw new Exception($"[Lex] Escape '\\{e}' no válido en char ({_line},{_col})");
                }
            }
            else
            {
                value = Advance();
            }

            if (IsAtEnd() || Peek() != '\'')
                throw new Exception($"[Lex] Char debe tener un solo carácter y cierre ' ({_line},{_col})");
            Advance(); // consumir cierre '
            Add(TokenKind.CharLit, value.ToString(), startLine, startCol);
        }

        // ---------- comentarios y espacios ----------
        private void SkipWhitespaceAndComments()
        {
            bool again;
            do
            {
                again = false;

                // espacios/tabs/nuevas líneas
                while (!IsAtEnd() && char.IsWhiteSpace(Peek())) Advance();

                // ~* ... *~
                if (Match("~*"))
                {
                    while (!IsAtEnd() && !Match("*~")) Advance();
                    if (IsAtEnd()) throw new Exception($"[Lex] Comentario ~*...*~ no terminado ({_line},{_col})");
                    again = true;
                }

                // ~ ... ~
                if (Match("~"))
                {
                    while (!IsAtEnd() && !Match("~")) Advance();
                    if (IsAtEnd()) throw new Exception($"[Lex] Comentario ~...~ no terminado ({_line},{_col})");
                    again = true;
                }
            } while (again);
        }

        // ---------- utilidades de lectura ----------
        private bool IsAtEnd() => _i >= _src.Length;
        private char Peek(int offset = 0)
        {
            int j = _i + offset;
            return j < _src.Length ? _src[j] : '\0';
        }
        private char Advance()
        {
            char c = _src[_i++];
            if (c == '\n') { _line++; _col = 1; } else { _col++; }
            return c;
        }
        private bool Match(string s)
        {
            for (int k = 0; k < s.Length; k++)
                if (_i + k >= _src.Length || _src[_i + k] != s[k])
                    return false;
            for (int k = 0; k < s.Length; k++) Advance();
            return true;
        }
        private void Add(TokenKind kind, string lexeme, int line, int col)
            => _tokens.Add(new Token(kind, lexeme, line, col));

        private static bool IsLetter(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        private static bool IsDigit(char c) => (c >= '0' && c <= '9');
        private static bool IsLetterOrDigit(char c) => IsLetter(c) || IsDigit(c);
    }
}
