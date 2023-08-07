declare class EditColumn extends DbColumn {
    editControlType?: string;
    pattern?: string;
    required?: boolean;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
