declare class EditColumn extends DbColumn {
    editControlType?: string;
    pattern?: string;
    browse?: boolean;
    required?: boolean;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
