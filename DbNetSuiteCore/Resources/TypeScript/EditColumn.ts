class EditColumn extends DbColumn{
    autoIncrement?: boolean;
    editControlType?: string;
    pattern?: string;
    required?: boolean;

    constructor(properties: EditColumnResponse, unmatched = false) {
        super();
        Object.keys(properties).forEach((key) => {
            if (properties[key as keyof EditColumnResponse] !== undefined)
                this[key as keyof EditColumnResponse] = properties[key as keyof EditColumnResponse] as any;
        });

        this.unmatched = unmatched;
    }
}