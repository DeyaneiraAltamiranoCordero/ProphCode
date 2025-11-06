using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Lexing
{
    public enum TokenKind
    {
        // ---- Literales e Identificadores 
        //Para poder identificar nombres de variables 
        Ident,            // nombres de variables/funciones
        IntLit,           // 123
        DecLit,           // 12.34
        TextLit,          // "hola"
        CharLit,          // 'a'
        BoolLit,          // lumus / nox (true/false)
        NullLit,          // null

        // ---- Palabras reservadas (Kw*)

        // Funciones / flujo principal
        KwSpell, KwEndSpell, KwAbracadabra, KwDisappear, KwReturn, KwSilence, KwInvoke,
        
        
        // Control
        KwIfSpellSay, KwIfFailSay, KwIfFail,
        KwDestinyChoose, KwPath, KwHiddenPath, KwBreak,
        KwAncestralLoop, KwEternalLoop, KwEternalLoopOnce,
        
        // E/S y lógica 
        KwReveal, KwAnd, KwOr, KwAnti,

        // Tipos / calificador
        KwProphecy, KwText, KwInt, KwDec, KwBool, KwList, KwVec,

        KwScry,   // función de entrada 
        KwStr, KwEnd, KwIgm, // para el for ancestral_loop: str/end/igm

        // ---- Operadores
        Plus, Minus, Star, Slash, Percent, // + - * / %
        EqEq, BangEq,                      // == !=
        Beyone, BeyoneEq, Under, UnderEq,  // 
        Arrow,                             // -> (asignación)

        // ---- Puntuación / delimitadores
        LParen, RParen, LBrace, RBrace, LBracket, RBracket,
        Semi, Comma, Colon,

        // ---- Especial
        //este es para saber que despues de dissaper ya no hay mas codigo. Esto es un " "
        Eof
    }
}
