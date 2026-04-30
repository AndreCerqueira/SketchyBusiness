from fastapi import FastAPI
import uvicorn
from pydantic import BaseModel
import ollama
import base64
from io import BytesIO
from quickdraw import QuickDrawData
from PIL import Image
import traceback

app = FastAPI(title="Pictionary AI Oponente Rápido")

print("A carregar o LLaVA para análise...")
print("Servidor pronto!")

qd_data = QuickDrawData()

class ImageRequest(BaseModel):
    ImageBase64: str
    Topic: str

class AiTextureRequest(BaseModel):
    Category: str 
    Word: str    

class JudgeRequest(BaseModel):
    PlayerImageBase64: str
    AiImageBase64: str
    Topic: str
    Word: str

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello! O oponente IA está pronto para jogar e julgar!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    prompt = (
        f"Act as the charismatic, slightly sarcastic host of the hit game show 'Sketchy Business'. "
        f"The image is a simple doodle drawn by a player trying to draw '{request.Topic}'. "
        f"Rules: "
        f"1. Start by making ONE very short, funny 'roast' about their drawing skills. "
        f"2. After the roast, guess what they were actually trying to draw. "
        f"3. Keep your response short (max 10 words), direct, and write it in English. "
        f"4. You MUST end your response by putting your final, single-word guess inside square brackets: [word]."
    )

    try:
        print("A processar a imagem do jogador com o LLaVA...")
        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    'images': [request.ImageBase64]
                }
            ]
        )
        descricao = resposta['message']['content']
        print(f"Resposta do LLaVA: {descricao}")

        return {
            "Description": descricao
        }
        
    except Exception as e:
        print("\n--- ERRO AO ANALISAR DESENHO ---")
        traceback.print_exc()
        print("--------------------------------\n")
        return {
            "Description": f"Ocorreu um erro a analisar o desenho: {str(e)}"
        }
    
@app.post("/judge-round")
def judge_round(request: JudgeRequest):
    prompt = (
        f"Act as the charismatic host and brutal sole judge of the game show 'Sketchy Business'. "
        f"The target word both players had to draw is '{request.Word}'. "
        f"Image 1 is the Player's drawing. Image 2 is the AI's drawing. "
        f"Rules: "
        f"1. As the judge, roast the Player's drawing and the AI's drawing in one short, cool sentence (max 12 words). "
        f"2. Decide who drew the '{request.Word}' better. "
        f"3. You MUST format your response EXACTLY as follows, replacing the content in the angle brackets:\n"
        f"Feedback: <your roast here>\n"
        f"Winner: <Player or AI>"
    )

    try:
        print("O LLaVA está a avaliar ambas as imagens para decidir o vencedor...")
        
        # Salvaguarda: limpar possíveis prefixos das strings base64 caso existam
        p_b64 = request.PlayerImageBase64.replace("data:image/png;base64,", "")
        a_b64 = request.AiImageBase64.replace("data:image/png;base64,", "")

        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    'images': [p_b64, a_b64]
                }
            ]
        )
        
        decisao = resposta['message']['content']
        print(f"Resposta Bruta do Juiz: {decisao}")
        
        feedback_texto = decisao
        vencedor_final = "None"
        
        if "Winner:" in decisao:
            partes = decisao.split("Winner:")
            feedback_texto = partes[0].replace("Feedback:", "").strip()
            vencedor_raw = partes[1].strip().lower()
            
            if "player" in vencedor_raw:
                vencedor_final = "Player"
            elif "ai" in vencedor_raw:
                vencedor_final = "AI"
        else:
            feedback_texto = decisao.replace("[Player]", "").replace("[AI]", "").strip()
            if "player" in decisao.lower():
                vencedor_final = "Player"
            elif "ai" in decisao.lower():
                vencedor_final = "AI"

        print(f"Feedback limpo para o TTS: {feedback_texto}")
        print(f"Vencedor da Ronda: {vencedor_final}")
        
        return {
            "Result": feedback_texto,
            "Winner": vencedor_final
        }
        
    except Exception as e:
        print("\n--- ERRO DETALHADO NO JULGAMENTO ---")
        traceback.print_exc()
        print("------------------------------------\n")
        return {
            "Result": "I am experiencing technical difficulties judging this round.", 
            "Winner": "None"
        }

@app.post("/generate-drawing")
def generate_drawing(request: AiTextureRequest):
    word_to_draw = request.Word.lower().strip()
    print(f"A IA está a gerar um desenho de '{word_to_draw}'...")
    
    try:
        doodle = qd_data.get_drawing(word_to_draw)
        
        # Converte a imagem para RGB para evitar problemas com transparências (RGBA) no base64
        pil_image = doodle.image.convert("RGB").resize((384, 384), Image.Resampling.LANCZOS)
        
        buffered = BytesIO()
        pil_image.save(buffered, format="PNG")
        img_str = base64.b64encode(buffered.getvalue()).decode("utf-8")
        
        print(f"Desenho de '{word_to_draw}' enviado!")
        return {"ImageBase64": img_str}
        
    except ValueError:
        print(f"Erro: A palavra '{word_to_draw}' não existe no dataset QuickDraw.")
        return {"ImageBase64": "ERROR: Word not found in doodle dataset"}
    except Exception as e:
        print(f"Erro a recuperar a imagem: {e}")
        return {"ImageBase64": ""}

@app.get("/generate-intro")
def generate_intro():
    prompt = (
        "Act as the charismatic, slightly sarcastic host and sole judge of the drawing game show 'Sketchy Business'. "
        "Welcome the audience to 'Sketchy Business'. State clearly: the first player to reach 7 points wins the ultimate cup. "
        "DO NOT mention any topic, word, or drawing phase yet. Be energetic and slightly sarcastic. Max 20 words. "
        "Write exactly ONE short sentence to say out loud. "
        "Do not include quotes, actions, or translations. Just the spoken text in English."
    )

    try:
        print("A gerar a introdução do jogo...")
        resposta = ollama.chat(
            model='llava',
            messages=[{'role': 'user', 'content': prompt}]
        )
        dialogue = resposta['message']['content'].replace('"', '').strip()
        print(f"Introdução gerada: {dialogue}")
        return {"Text": dialogue}
    except Exception as e:
        print(f"Erro a gerar introdução: {e}")
        return {"Text": ""}
    

if __name__ == "__main__":
    print("A iniciar o servidor FastAPI...")
    # Podes mudar a porta (8000) se for necessário
    uvicorn.run(app, host="127.0.0.1", port=8000)