using ProphCode.Core.Lexing;

namespace ProphCode.Core.Types
{
    public enum TypeId { Int, Dec, Text, Bool }

    public static class TypeHelpers
    {
        public static bool TryFromKeyword(TokenKind k, out TypeId t)
        {
            switch (k)
            {
                case TokenKind.KwInt: t = TypeId.Int; return true;
                case TokenKind.KwDec: t = TypeId.Dec; return true;
                case TokenKind.KwText: t = TypeId.Text; return true;
                case TokenKind.KwBool: t = TypeId.Bool; return true;
                default: t = default; return false;
            }
        }

        public static string ToKeyword(TypeId t) => t switch
        {
            TypeId.Int => "int",
            TypeId.Dec => "dec",
            TypeId.Text => "text",
            TypeId.Bool => "bool",
            _ => "<?>"
        };

        // Reglas de compatibilidad literal ← tipo
        public static bool LiteralMatches(TypeId t, TokenKind lit)
        {
            return t switch
            {
                TypeId.Int => lit == TokenKind.IntLit,
                TypeId.Dec => lit == TokenKind.DecLit || lit == TokenKind.IntLit, // permitir 5 en dec
                TypeId.Text => lit == TokenKind.TextLit,
                TypeId.Bool => lit == TokenKind.BoolLit,
                _ => false
            };
        }
    }
}
