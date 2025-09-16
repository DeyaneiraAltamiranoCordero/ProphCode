SNIPPETS = {
    "palabras_reservadas": """# Palabras reservadas (ejemplo)
# program, end, si, entonces, sino, mientras, hacer,
# funcion, retornar, var, imprimir, verdadero, falso
program MiPrograma
    # tu código aquí
end
""",
    "control": """# Sintaxis: Estructuras de control
program ControlDemo
    var x = 3
    si x > 2 entonces
        imprimir "x es mayor que 2"
    sino
        imprimir "x no es mayor que 2"
    end
end
""",
    "funciones": """# Sintaxis: Funciones
program FuncionesDemo
    funcion suma(a, b)
        retornar a + b
    end

    var r = suma(2, 5)
    imprimir "Resultado: " + r
end
""",
    "operaciones": """# Sintaxis: Operaciones
program OperacionesDemo
    var a = 10
    var b = 4
    var c = (a + b) * 2 - 3
    imprimir "c = " + c
end
""",
    "semantica": """# Semántica (comentado)
# - Tipos simples: numero, booleano, texto (string)
# - + suma números o concatena textos si hay al menos un texto
# - Comparadores: ==, !=, >, <, >=, <= producen booleano
# - 'si ... entonces ... sino ... end' evalúa un booleano
# - 'funcion' crea un ámbito con parámetros y 'retornar' finaliza la función
""",
    "tipos": """# Tipos de datos
program TiposDemo
    var n = 42            # numero
    var t = "hola"        # texto
    var b = verdadero     # booleano
    imprimir "n=" + n + ", t=" + t + ", b=" + b
end
"""
}
