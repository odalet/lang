import dearpygui.dearpygui as dpg


# See https://github.com/hoffstadt/DearPyGui/discussions/1002
def show_info(title, message, selection_callback):
    # guarantee these commands happen in the same frame
    with dpg.mutex():
        viewport_width = dpg.get_viewport_client_width()
        viewport_height = dpg.get_viewport_client_height()

        # with dpg.window(label=title, modal=True, no_close=True) as modal_id:
        #     dpg.add_text(message)
        #     dpg.add_button(label="Ok", width=75, user_data=(modal_id, True), callback=selection_callback)
        #     dpg.add_same_line()
        #     dpg.add_button(label="Cancel", width=75, user_data=(modal_id, False), callback=selection_callback)

        with dpg.window(label=title, modal=True, no_close=True) as modal_id:
            dpg.add_text(message)
            with dpg.group(horizontal=True):
                dpg.add_button(label="Ok", width=75, user_data=(modal_id, True), callback=selection_callback)
                dpg.add_button(label="Cancel", width=75, user_data=(modal_id, False), callback=selection_callback)

    # guarantee these commands happen in another frame
    dpg.split_frame()
    width = dpg.get_item_width(modal_id)
    height = dpg.get_item_height(modal_id)
    dpg.set_item_pos(modal_id, [viewport_width // 2 - width // 2, viewport_height // 2 - height // 2])


def main() -> None:
    dpg.create_context()
    dpg.create_viewport(title='Custom Title', width=600, height=300)

    with dpg.window(label="Example Window", on_close=lambda: show_info("Question", "Exit?", None)):
        dpg.add_text("Hello, world")
        dpg.add_button(label="Save")
        dpg.add_input_text(label="string", default_value="Quick brown fox")
        dpg.add_slider_float(label="float", default_value=0.273, max_value=1)

    dpg.setup_dearpygui()
    dpg.show_viewport()
    dpg.start_dearpygui()
    dpg.destroy_context()


if __name__ == '__main__':
    main()
