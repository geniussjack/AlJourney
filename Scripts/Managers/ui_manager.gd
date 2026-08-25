extends Node
## Global (autoload) UI manager. Responsible for opening, closing and
## managing the stack of menu screens.

## Raised when a new menu is opened.
signal menu_opened(menu_name: String)
## Raised when a menu is closed.
signal menu_closed(menu_name: String)

var _menu_stack: Array[Control] = []
var _current_menu: Control = null

## Opens the given menu, hiding the current one and pushing it onto the back stack.
func open_menu(menu: Control) -> void:
	if menu == null:
		printerr("[UIManager] Cannot open null menu")
		return

	if _current_menu != null:
		_menu_stack.append(_current_menu)
		_current_menu.hide()

	_current_menu = menu
	_current_menu.show()

	menu_opened.emit(menu.name)
	print("[UIManager] Opened menu: %s" % menu.name)

## Closes the currently active menu and returns to the previous menu on the stack.
func close_current_menu() -> void:
	if _current_menu == null:
		printerr("[UIManager] No menu to close")
		return

	var menu_name: String = _current_menu.name
	_current_menu.hide()

	menu_closed.emit(menu_name)
	print("[UIManager] Closed menu: %s" % menu_name)

	if not _menu_stack.is_empty():
		_current_menu = _menu_stack.pop_back()
		_current_menu.show()
	else:
		_current_menu = null

## Closes every open menu and clears the back stack.
func close_all_menus() -> void:
	if _current_menu != null:
		_current_menu.hide()
	_current_menu = null

	while not _menu_stack.is_empty():
		var menu: Control = _menu_stack.pop_back()
		menu.hide()

	print("[UIManager] All menus closed")
