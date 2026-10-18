export type Option = { value: string; label: string };

// What the two kinds of select (the menu for a mouse, the sheet for a touch screen) both receive.
export type ControlProps = {
  id: string;
  // The field's label. A sheet uses it as its title.
  label: string;
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
  onBlur?: () => void;
  disabled?: boolean;
  // What the field says while nothing is chosen, e.g. "Choose a category".
  placeholder?: string;
  invalid: boolean;
  describedBy?: string;
};
