/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class UploadDialog extends Dialog {
    parent: DbNetEdit;
    inputFile: HTMLInputElement;
    $apply: JQuery<HTMLButtonElement>;
    file: File | undefined;
    $uploadButton: JQuery<HTMLButtonElement> | undefined;
    $previewImage: JQuery<HTMLImageElement>;
    $editImage: JQuery<HTMLImageElement> | undefined;
    fileMetaData: FileMetaData | undefined;
    $selectFilesButton: JQuery<HTMLButtonElement>;
    constructor(id: string, parent: DbNetEdit);
    private dialogOpened;
    private sizeDialog;
    show(event: JQuery.TriggeredEvent): void;
    private previewLoaded;
    private selectFile;
    private fileSelected;
    private clear;
    private renderFileAsImage;
    private renderFileAsNonImage;
    private apply;
}
