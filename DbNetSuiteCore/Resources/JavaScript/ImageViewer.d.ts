/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
declare class ImageViewer extends Dialog {
    $image: JQuery<HTMLImageElement>;
    $video: JQuery<HTMLVideoElement>;
    $audio: JQuery<HTMLAudioElement>;
    $download: JQuery<HTMLAnchorElement>;
    fileName: string;
    fileType: string;
    constructor(id: string);
    show(src: string, fileName: string, type?: string): void;
    private openViewer;
    private download;
}
