type TextTransform = "uppercase" | "lowercase" | "capitalize";
declare class EditColumn extends DbColumn {
    annotation?: string;
    autoIncrement?: boolean;
    editControlType?: string;
    pattern?: string;
    placeholder?: string;
    required?: boolean;
    readOnly?: boolean;
    inputValidation?: object;
    textTransform?: TextTransform;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
