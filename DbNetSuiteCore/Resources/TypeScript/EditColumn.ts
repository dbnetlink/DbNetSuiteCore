type TextTransform = "uppercase" | "lowercase" | "capitalize"
class EditColumn extends DbColumn{
    annotation?: string;
    autoIncrement?: boolean;
    editControlType?: string;
    pattern?: string;
    placeholder?: string;
    required?: boolean;
    readOnly?: boolean;
    inputValidation?: object;
    textTransform?: TextTransform;
    defaultValue?: string;

    constructor(properties: EditColumnResponse, unmatched = false) {
        super();
        Object.keys(properties).forEach((key) => {
            if (properties[key as keyof EditColumnResponse] !== undefined)
                this[key as keyof EditColumnResponse] = properties[key as keyof EditColumnResponse] as any;
        });

        this.unmatched = unmatched;
    }
}