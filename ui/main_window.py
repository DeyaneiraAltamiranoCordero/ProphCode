from PyQt5.QtCore import Qt
from PyQt5.QtWidgets import (
    QMainWindow, QDockWidget, QToolBar, QAction, QMessageBox
)
from .editor import create_editor
from .output import create_output
from .menu import build_language_menu

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Mini IDE • Prototipo (modular)")
        self.resize(1000, 650)

        # Editor central (archivo separado)
        self.editor = create_editor()
        self.setCentralWidget(self.editor)

        # Panel de salida (dock inferior, archivo separado)
        self.output = create_output()
        dock = QDockWidget("Salida / Errores", self)
        dock.setWidget(self.output)
        dock.setObjectName("dock_output")
        dock.setAllowedAreas(Qt.BottomDockWidgetArea)
        self.addDockWidget(Qt.BottomDockWidgetArea, dock)

        # Barra de herramientas
        toolbar = QToolBar("Acciones")
        self.addToolBar(toolbar)

        act_compilar = QAction("Compilar", self)
        act_compilar.triggered.connect(self.compilar)
        toolbar.addAction(act_compilar)

        act_ejecutar = QAction("Ejecutar", self)
        act_ejecutar.triggered.connect(self.ejecutar)
        toolbar.addAction(act_ejecutar)

        # Menú 'Estructuras del lenguaje' (archivo separado)
        build_language_menu(self)

        self.statusBar().showMessage("Listo")

    # --- helpers ---
    def insertar(self, texto: str):
        cursor = self.editor.textCursor()
        cursor.insertText(texto)
        self.editor.setTextCursor(cursor)
        self.statusBar().showMessage("Snippet insertado", 2000)

    # --- compilación (chequeos mínimos de ejemplo) ---
    def compilar(self):
        code = self.editor.toPlainText()
        errores = []
        lineas = code.splitlines()

        if "program" not in code:
            errores.append("Falta palabra reservada: 'program'")
        if "end" not in code:
            errores.append("Falta palabra reservada de cierre: 'end'")

        balance = 0
        for ch in code:
            if ch == "(":
                balance += 1
            elif ch == ")":
                balance -= 1
                if balance < 0:
                    errores.append("Paréntesis desbalanceados: ')' extra")
                    break
        if balance > 0:
            errores.append("Paréntesis desbalanceados: falta ')'")

        if "si " in code and "entonces" in code and "end" not in code:
            errores.append("Bloque 'si ... entonces' sin 'end' de cierre")

        for idx, ln in enumerate(lineas, start=1):
            if ln.strip().startswith("imprimir") and ln.strip() == "imprimir":
                errores.append(f"L{idx}: 'imprimir' sin argumento")

        self.output.clear()
        if errores:
            self.output.appendPlainText("✗ Errores de compilación:")
            for e in errores:
                self.output.appendPlainText(f"  - {e}")
            self.statusBar().showMessage("Compilación: errores encontrados", 3000)
        else:
            self.output.appendPlainText("✓ Compilación exitosa (validaciones mínimas).")
            self.statusBar().showMessage("Compilación exitosa", 3000)

    # --- ejecución (intérprete DEMO súper básico) ---
    def ejecutar(self):
        code = self.editor.toPlainText()
        lineas = [ln.strip() for ln in code.splitlines()]
        vars_ = {}
        salida = []
        errores = []

        def parse_val(x):
            x = x.strip()
            if x.lower() in ("verdadero", "falso"):
                return True if x.lower() == "verdadero" else False
            if x.startswith('"') and x.endswith('"'):
                return x[1:-1]
            try:
                if "." in x:
                    return float(x)
                return int(x)
            except ValueError:
                return vars_.get(x, f"<{x}?>")

        i = 0
        while i < len(lineas):
            ln = lineas[i]
            i += 1
            if not ln or ln.startswith("#"):
                continue

            if ln.startswith("var "):
                try:
                    _, rest = ln.split("var ", 1)
                    nombre, valor = rest.split("=", 1)
                    nombre = nombre.strip()
                    valor = parse_val(valor)
                    vars_[nombre] = valor
                except Exception:
                    errores.append(f"Error asignando variable en: '{ln}'")
                continue

            if ln.startswith("imprimir"):
                try:
                    expr = ln[len("imprimir"):].strip()
                    if "+" in expr:
                        partes = [p.strip() for p in expr.split("+")]
                        acc = ""
                        num_acc = 0
                        as_text = False
                        for p in partes:
                            val = parse_val(p)
                            if isinstance(val, str) or isinstance(acc, str) or isinstance(val, bool):
                                as_text = True
                            if as_text:
                                acc = (str(acc) if acc != "" else "") + str(val)
                            else:
                                try:
                                    num_acc = (num_acc if acc == "" else acc) + float(val)
                                    acc = num_acc
                                except Exception:
                                    acc = str(acc) + str(val)
                                    as_text = True
                        salida.append(str(acc))
                    else:
                        salida.append(str(parse_val(expr)))
                except Exception:
                    errores.append(f"Error en imprimir: '{ln}'")
                continue

            if ln.startswith("si ") and " entonces" in ln:
                cond_txt = ln[3: ln.index(" entonces")].strip()
                cond = False
                try:
                    ops = [">=", "<=", "==", "!=", ">", "<"]
                    op = next((o for o in ops if o in cond_txt), None)
                    if op:
                        a, b = [t.strip() for t in cond_txt.split(op)]
                        av = parse_val(a)
                        bv = parse_val(b)
                        if op == ">=": cond = av >= bv
                        elif op == "<=": cond = av <= bv
                        elif op == "==": cond = av == bv
                        elif op == "!=": cond = av != bv
                        elif op == ">":  cond = av >  bv
                        elif op == "<":  cond = av <  bv
                    else:
                        cond = bool(parse_val(cond_txt))
                except Exception:
                    errores.append(f"Condición inválida: '{cond_txt}'")

                bloque = []
                while i < len(lineas) and lineas[i] != "end":
                    bloque.append(lineas[i])
                    i += 1
                if i < len(lineas) and lineas[i] == "end":
                    i += 1  # saltar 'end'
                if cond:
                    for b in bloque:
                        if b.startswith("var "):
                            try:
                                _, rest = b.split("var ", 1)
                                nombre, valor = rest.split("=", 1)
                                vars_[nombre.strip()] = parse_val(valor)
                            except Exception:
                                errores.append(f"Error asignando variable en: '{b}'")
                        elif b.startswith("imprimir"):
                            try:
                                expr = b[len("imprimir"):].strip()
                                if "+" in expr:
                                    partes = [p.strip() for p in expr.split("+")]
                                    acc = ""
                                    num_acc = 0
                                    as_text = False
                                    for p in partes:
                                        val = parse_val(p)
                                        if isinstance(val, str) or isinstance(acc, str) or isinstance(val, bool):
                                            as_text = True
                                        if as_text:
                                            acc = (str(acc) if acc != "" else "") + str(val)
                                        else:
                                            try:
                                                num_acc = (num_acc if acc == "" else acc) + float(val)
                                                acc = num_acc
                                            except Exception:
                                                acc = str(acc) + str(val)
                                                as_text = True
                                    salida.append(str(acc))
                                else:
                                    salida.append(str(parse_val(expr)))
                            except Exception:
                                errores.append(f"Error en imprimir: '{b}'")
                continue

            if ln.startswith("funcion ") or ln.startswith("retornar"):
                continue

            if ln == "end" or ln.startswith("program"):
                continue

        self.output.clear()
        if errores:
            self.output.appendPlainText("⚠️ Errores en ejecución:")
            for e in errores:
                self.output.appendPlainText(f"  - {e}")
        if salida:
            self.output.appendPlainText("▶ Salida del programa:")
            for s in salida:
                self.output.appendPlainText(f"  {s}")
        if not errores and not salida:
            self.output.appendPlainText("✔ Ejecución sin errores (sin salida).")
        self.statusBar().showMessage("Ejecución finalizada", 3000)
