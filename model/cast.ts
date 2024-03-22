/**
 * cast definitions
 */
interface Definitions{
	version: string;
	casts: Cast[];
}

/**
 * CeVIO cast (voice library)
 */
interface Cast{
	/** internal cast id */
	id: string;
	/** internal cast name */
	cname: string;
	/** display cast names */
	names: DisplayName[];
	/** cast category (song voice or talk voice) */
	category: SongVoice | TalkVoice;
	/** software */
	product: "CeVIO CS" | "CeVIO AI" | "VoiSona";
	/** voice gender */
	gender: "Male" | "Female";
	/** voice language */
	lang: Language;
	/** voice libraries versions list */
	versions?: VoiceVersion[];
	/** True/false value of whether this cast has emotion parameters */
	hasEmotions: boolean;
	emotions?: Emotion[];
	emotionOrder?: EmotionOrder[];
	/** special label definitions */
	symbols?: SpSymbol[];
}

/**
 * Display of name in each language
 */
interface DisplayName{
	/** display language */
	lang: Language;
	/** display name */
	display: string;
}

type Language = "Japanese" | "English" | "Chinese";

/**
 * Type def for Song voice casts
 */
type SongVoice = "Song" | "SingerSong";

/**
 * Type def for Talk voice casts
 */
type TalkVoice = "Talk" | "TextVocal";

/**
 * voice's version
 */
type VoiceVersion = `${number}.${number}.${number}`;

/**
 * Emotion parameter
 */
interface Emotion{
	id: string;
	names?: DisplayName[];
}

/**
 * Emotion order parameter
 */
interface EmotionOrder{
	version: VoiceVersion;
	order: string[];
}

/**
 * Special label parameter
 */
interface SpSymbol{
	id: string;
	symbols: string[];
	names: DisplayName[];
}

/*
type IdLang = "JP" | "EN";
type IdGender = "F" | "M";
type IdNumber = number;
type IdTalkVenderAI = "AHS" | "FP" | "USTL" | "ZNSN";
type IdSongVenderCS = "XSV";
type IdSongVenderAI = "AHS" | "FP" | "VCLMK" | "THR" | "KZA" | "GLA" | "BSR"
type IdTalkCastCS = "A" | "B" | "C" | "ONE" | "CTV-JPF-FP2";
type IdTalkCastAI = `${IdTalkVenderAI}${IdNumber}` | "A" | "B" | "C";
type IdSongCastCS = "W" | "E" | "P" | "HRM-P" | "A" | "ONE" | `FP3-${"N" | "P"}`
type IdSongCastAI = `${IdSongVenderAI}${IdNumber}` | "A" | "B";

type IdCastFull = IdCastFullCS | IdCastFullAI | IdCastFullVoiSona;
type IdCastFullCS = IdCastFullCSSong | IdCastFullCSTalk;
type IdCastFullCSSong = `${IdSongVenderCS}-JP${IdGender}-${"W" | "E" | "P" | "HRM-P"}` | `CSV-ENF-FP3-${"N" | "P"}` | "ONE" | "A";
type IdCastFullCSTalk = IdTalkCastCS;
type IdCastFullAI = "";
type IdCastFullAISong = `CSNV-${IdLang}${IdGender}-${IdTalkCastAI}`;
type IdCastFullAITalk = `CTNV-${IdLang}${IdGender}-${IdTalkCastAI}`;
type IdCastFullVoiSona = "";
*/

/**
 * CeVIO CS talk voices emotion id names
 */
/*
type EmotionIdsTalkCS =
	"active" |
	"angry" |
	"bashful" |
	"cheerful" |
	"cool" |
	"depression" |
	"fine" |
	"irritated" |
	"mild" |
	"normal" |
	"ordinary" |
	"sad" |
	"sorrow" |
	//IA Talk only
	`CTV-JPF-FP2_${"Fine" | "Normal" | "Angry" | "Sad"}`;
*/
/**
 * CeVIO AI talk voices emotion id names
 */
/*
type EmotionIdsTalkAI =
	`CTNV-${IdLang}${IdGender}-${IdTalkCastAI}_${EmotionNamesAiTalk}`;

type EmotionNamesAiTalk =
	"ANGRY" |
	"BASHFUL" |
	"BRIGHT" |
	"CALM" |
	"COOL" |
	"DARK" |
	"FINE" |
	"HAPPY" |
	"MILD" |
	"NORMAL" |
	"SAD" |
	"STRONG" |
	"WHISPER"
	;
*/