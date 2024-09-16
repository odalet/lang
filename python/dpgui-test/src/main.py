# import dearpygui.dearpygui as dpg
# import dpg_ui
import imgui_bundle_ui as imgui

def main() -> None:
    #print("Hello World")

    # imgui bundle Tests
    #imgui.run_imgui_bundle_simple()
    imgui.integration.run_main()

    # dearpygui Tests
    # dpg.configure_app(docking=True, docking_space=False)
    # # dpg_ui.show_simple_ui()
    # dpg_ui.run_demo()


if __name__ == '__main__':
    main()
