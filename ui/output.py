from PyQt5.QtWidgets import QPlainTextEdit

def create_output():
    out = QPlainTextEdit()
    out.setReadOnly(True)
    return out
