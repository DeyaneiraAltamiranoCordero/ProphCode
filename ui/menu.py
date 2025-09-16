from PyQt5.QtWidgets import QMenu, QAction
from .snippets import SNIPPETS

def build_language_menu(window):
    """
    Crea el menú 'Estructuras del lenguaje' en window.menuBar()
    y conecta cada acción para insertar el snippet correspondiente.
    Requiere que window tenga un método window.insertar(texto: str).
    """
    menu = window.menuBar().addMenu("Estructuras del lenguaje")

    act_palabras = QAction("Palabras reservadas", window)
    act_palabras.triggered.connect(lambda: window.insertar(SNIPPETS["palabras_reservadas"]))
    menu.addAction(act_palabras)

    sub_sintaxis = QMenu("Sintaxis", window)

    act_control = QAction("Control", window)
    act_control.triggered.connect(lambda: window.insertar(SNIPPETS["control"]))
    sub_sintaxis.addAction(act_control)

    act_funciones = QAction("Funciones", window)
    act_funciones.triggered.connect(lambda: window.insertar(SNIPPETS["funciones"]))
    sub_sintaxis.addAction(act_funciones)

    act_operaciones = QAction("Operaciones", window)
    act_operaciones.triggered.connect(lambda: window.insertar(SNIPPETS["operaciones"]))
    sub_sintaxis.addAction(act_operaciones)

    menu.addMenu(sub_sintaxis)

    act_semantica = QAction("Semántica", window)
    act_semantica.triggered.connect(lambda: window.insertar(SNIPPETS["semantica"]))
    menu.addAction(act_semantica)

    act_tipos = QAction("Tipos de datos", window)
    act_tipos.triggered.connect(lambda: window.insertar(SNIPPETS["tipos"]))
    menu.addAction(act_tipos)

    return menu
