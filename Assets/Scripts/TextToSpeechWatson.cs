using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;

public class TextToSpeechWatson
{

    public string TextToSynth;
    TextToSpeech m_TextToSpeech = new TextToSpeech();
//    string m_TestString = "Hello! This is Text to Speech!";


   public void Start()
    {
//    print(m_TestString);
    m_TextToSpeech.ToSpeech(TextToSynth, HandleToSpeechCallback);

    }
        
	public void HandleToSpeechCallback(AudioClip clip)
	{

		PlayClip(clip);
	}

	bool clipisplaying = false;
	GameObject clipobj = null;

    public void PlayClip(AudioClip clip)
    {
		if (clipisplaying && clipobj!= null) {
			GameObject.Destroy(clipobj);
			clipobj = null;
			clipisplaying = false;
		}

		if (Application.isPlaying && clip != null && AppConfig.Settings.Frontend.playTTS){
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
			source.loop = false;
            source.clip = clip;
            source.Play();
			clipisplaying = true;

			clipobj = audioObject;

	        GameObject.Destroy(audioObject, clip.length);

        }
    }
		
}