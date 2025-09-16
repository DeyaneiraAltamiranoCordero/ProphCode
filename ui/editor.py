from PyQt5.QtWidgets import QPlainTextEdit

def create_editor():
    editor = QPlainTextEdit()
    editor.setPlaceholderText("Escribe tu código aquí…")
    return editor
