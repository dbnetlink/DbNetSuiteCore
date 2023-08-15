/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class ImageViewer extends Dialog {
    $image: JQuery<HTMLImageElement>;
    $download: JQuery<HTMLAnchorElement>;
    fileName: string;
    constructor(id: string);
    show($img: JQuery<HTMLImageElement>): void;
    private download;
}
