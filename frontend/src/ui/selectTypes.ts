export type Option = { value: string; label: string };

// What the two kinds of select (our menu, and the phone's own picker) both receive.
export type ControlProps = {
  id: string;
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
  onBlur?: () => void;
  disabled?: boolean;
  invalid: boolean;
  describedBy?: string;
};
