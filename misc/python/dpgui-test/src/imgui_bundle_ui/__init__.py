from imgui_bundle import imgui, immapp
import imgui_bundle_ui.integration
# from integration import *


def gui() -> None:
    imgui.text("Hello, world!")


def run_imgui_bundle_simple() -> None:
    immapp.run(
        gui_function=gui,  # The Gui function to run
        window_title="Hello!",  # the window title
        window_size_auto=True,  # Auto size the application window given its widgets
        # Uncomment the next line to restore window position and size from previous run
        # window_restore_previous_geometry==True
    )


if __name__ == '__main__':
    pass
