using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Lexing
{
    public enum TokenKind
    {
        Ident,            // nombres de variables/funciones
        IntLit,           // 123
        DecLit,           // 12.34
        TextLit,          // "hola"
        CharLit,          // 'a'
        BoolLit,          // lumus / nox (true/false)
        NullLit,          // null


        // Funciones / flujo principal
        KwSpell, KwEndSpell, KwAbracadabra, KwDisappear, KwReturn, KwSilence, KwInvoke,
        
        
        // Control
        KwIfSpellSay, KwIfFailSay, KwIfFail,
        KwDestinyChoose, KwPath, KwHiddenPath, KwBreak,
        KwAncestralLoop, KwEternalLoop, KwEternalLoopOnce,
        
        // Secuencias lógica 
        KwReveal, KwAnd, KwOr, KwAnti,

        // Tipos
        KwProphecy, KwText, KwInt, KwDec, KwBool, KwList, KwVec, KwChar,

        KwScry,   // función de entrada 
        KwStr, KwEnd, KwIgm, // para el for ancestral_loop

        //  Operadores
        Plus, Minus, Star, Slash, Percent, // + - * / %
        EqEq, BangEq,                      // == !=
        Beyone, BeyoneEq, Under, UnderEq,  // 
        Arrow,                             // -> 

        //  Puntuación / delimitadores
        LParen, RParen, LBrace, RBrace, LBracket, RBracket,
        Semi, Comma, Colon,

        Eof
    }
}