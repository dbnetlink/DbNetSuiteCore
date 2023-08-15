declare class EditColumn extends DbColumn {
    autoIncrement?: boolean;
    editControlType?: string;
    pattern?: string;
    required?: boolean;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
