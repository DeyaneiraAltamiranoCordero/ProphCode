using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProphCode.Core.Lexing
{
    public static class Words
    {
        public static readonly IDictionary<string, TokenKind> Map =
            new Dictionary<string, TokenKind> /*(StringComparer.Ordinal) si quieres forzar case-sensitive explícito*/
            {
                // ---- Operadores con palabra
                ["beyone"] = TokenKind.Beyone,     // >
                ["beyoneq"] = TokenKind.BeyoneEq,   // >=
                ["under"] = TokenKind.Under,      // <
                ["undereq"] = TokenKind.UnderEq,    // <=

                // ---- Literales por palabra
                ["lumus"] = TokenKind.BoolLit,      // true
                ["nox"] = TokenKind.BoolLit,      // false
                ["null"] = TokenKind.NullLit,      // null

                // ---- Palabras reservadas (Kw*)
                // Funciones / flujo principal
                ["spell"] = TokenKind.KwSpell,
                ["endSpell"] = TokenKind.KwEndSpell,
                ["abracadabra"] = TokenKind.KwAbracadabra,
                ["disappear"] = TokenKind.KwDisappear,
                ["return"] = TokenKind.KwReturn,
                ["silence"] = TokenKind.KwSilence,
                ["invoke"] = TokenKind.KwInvoke,

                // Control
                ["if_spell_say"] = TokenKind.KwIfSpellSay,
                ["if_fail_say"] = TokenKind.KwIfFailSay,
                ["if_fail"] = TokenKind.KwIfFail,
                ["destiny_choose"] = TokenKind.KwDestinyChoose,
                ["path"] = TokenKind.KwPath,
                ["hidden_path"] = TokenKind.KwHiddenPath,
                ["break"] = TokenKind.KwBreak,
                ["ancestral_loop"] = TokenKind.KwAncestralLoop,
                ["eternal_loop"] = TokenKind.KwEternalLoop,
                ["eternal_loop_once"] = TokenKind.KwEternalLoopOnce,
                ["scry"] = TokenKind.KwScry,


                // E/S y lógicas por palabra
                ["reveal"] = TokenKind.KwReveal,
                ["and"] = TokenKind.KwAnd,
                ["or"] = TokenKind.KwOr,
                ["anti"] = TokenKind.KwAnti,

                // Tipos / calificador
                ["prophecy"] = TokenKind.KwProphecy,
                ["text"] = TokenKind.KwText,
                ["int"] = TokenKind.KwInt,
                ["dec"] = TokenKind.KwDec,
                ["bool"] = TokenKind.KwBool,
                ["list"] = TokenKind.KwList,
                ["vec"] = TokenKind.KwVec,
                ["char"] = TokenKind.KwChar,

                // Ancestral_lopp

                ["str"] = TokenKind.KwStr,
                ["end"] = TokenKind.KwEnd,
                ["igm"] = TokenKind.KwIgm,
            };
    }
}

