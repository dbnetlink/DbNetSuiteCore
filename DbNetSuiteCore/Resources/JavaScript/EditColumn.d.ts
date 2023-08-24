declare class EditColumn extends DbColumn {
    autoIncrement?: boolean;
    editControlType?: string;
    pattern?: string;
    required?: boolean;
    readOnly?: boolean;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
