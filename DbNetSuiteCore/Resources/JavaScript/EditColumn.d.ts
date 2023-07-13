declare class EditColumn extends DbColumn {
    editControlType?: string;
    pattern?: string;
    constructor(properties: EditColumnResponse, unmatched?: boolean);
}
