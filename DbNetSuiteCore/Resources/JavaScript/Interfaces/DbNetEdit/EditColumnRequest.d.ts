interface EditColumnRequest extends GridColumnRequest {
    editControlType?: string;
    pattern?: string;
    required?: boolean;
    autoIncrement?: boolean;
    readOnly?: boolean;
    annotation?: string;
    placeholder: string;
    inputValidation: object;
    textTransform?: TextTransform;
}
