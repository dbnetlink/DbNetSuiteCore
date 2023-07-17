interface ValidationMessage {
    key: string,
    value: string;
}

interface DbNetEditResponse extends DbNetGridEditResponse {
    form: string;
    currentRow: number;
    columns?: EditColumn[];
    primaryKey?: string;
    validationMessage?: ValidationMessage;
}