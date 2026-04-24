from fastapi import FastAPI
from pydantic import BaseModel
import ollama
import base64
from io import BytesIO
from quickdraw import QuickDrawData # Nova biblioteca para rabiscos instantâneos
from PIL import Image

app = FastAPI(title="Pictionary AI Oponente Rápido")

print("A carregar o LLaVA para análise...")
# Ollama/LLaVA continua a ser necessário para a IA adivinhar o TEU desenho.
# Mas não precisamos de carregar o Stable Diffusion aqui.
print("LLaVA pronto!")

# Inicializamos o acesso aos dados do Quick Draw uma vez ao iniciar
qd_data = QuickDrawData()

class ImageRequest(BaseModel):
    ImageBase64: str
    Topic: str

class AiTextureRequest(BaseModel):
    Category: str # O tema geral
    Word: str     # A palavra exata a desenhar

class JudgeRequest(BaseModel):
    PlayerImageBase64: str
    AiImageBase64: str
    Topic: str
    Word: str

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello! O oponente IA está pronto para desenhar rápido!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    # --- MANTEMOS O TEU CÓDIGO ORIGINAL DO LLAVA AQUI ---
    prompt = (
        f"Act as a sassy and sarcastic player in a Pictionary game. "
        f"The image is a simple doodle drawn by a player trying to draw something related to the topic '{request.Topic}'. "
        f"Rules: "
        f"1. Start by making ONE very short, funny 'roast' (a sarcastic comment) about how bad, chaotic, or weird the player's drawing skills are. "
        f"2. After the roast, guess what the player was actually trying to draw within the topic '{request.Topic}'. "
        f"3. Keep your response short, direct, and write it in English."
        f"4. You MUST end your response by putting your final, single-word guess inside square brackets, exactly like this: [word]."
    )

    try:
        print("A processar a imagem com o LLaVA. Aguarda...")
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
        return {"Description": descricao}
        
    except Exception as e:
        return {"Description": f"Ocorreu um erro a analisar o desenho: {str(e)}"}
    
@app.post("/judge-round")
def judge_round(request: JudgeRequest):
    # O LLaVA recebe as imagens por ordem. A primeira na lista será a Imagem 1, a segunda a Imagem 2.
    prompt = (
        f"Act as a sassy and sarcastic Pictionary judge. "
        f"The topic is '{request.Topic}' and the target word both players had to draw is '{request.Word}'. "
        f"You will see two images: Image 1 is the Player's drawing. Image 2 is the AI's drawing. "
        f"Rules: "
        f"1. Roast the Player's drawing (Image 1) and then the AI's drawing (Image 2) in one short brutal sentence (max 10 words each). "
        f"3. Decide who drew the '{request.Word}' better. "
        f"4. You MUST end your response EXACTLY with the winner inside square brackets: [Player] or [AI]."
    )

    try:
        print("O LLaVA está a avaliar ambas as imagens para decidir o vencedor...")
        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    # Passamos a imagem do jogador e depois a da IA
                    'images': [request.PlayerImageBase64, request.AiImageBase64]
                }
            ]
        )
        decisao = resposta['message']['content']
        print(f"Decisão do Juiz: {decisao}")
        return {"Result": decisao}
        
    except Exception as e:
        return {"Result": f"Erro ao julgar a ronda: {str(e)}"}

@app.post("/generate-drawing")
def generate_drawing(request: AiTextureRequest):
    # A palavra precisa de estar em inglês e normalizada (lowercase) para o QuickDraw
    word_to_draw = request.Word.lower().strip()
    print(f"IA Oponente está a 'desenhar' um(a) '{word_to_draw}' instantaneamente...")
    
    try:
        doodle = qd_data.get_drawing(word_to_draw)
        
        pil_image = doodle.image.resize((384, 384), Image.Resampling.LANCZOS)
        
        # Converter para Base64
        buffered = BytesIO()
        pil_image.save(buffered, format="PNG")
        img_str = base64.b64encode(buffered.getvalue()).decode("utf-8")
        
        print(f"Desenho de '{word_to_draw}' recuperado e enviado!")
        return {"ImageBase64": img_str}
        
    except ValueError:
        print(f"Erro: A palavra '{word_to_draw}' não existe no dataset QuickDraw.")
        # Se a palavra não existir, podes retornar uma imagem em branco ou 
        # um erro que o Unity trate, pedindo ao jogador para escolher outra palavra.
        return {"ImageBase64": "ERROR: Word not found in doodle dataset"}
    except Exception as e:
        print(f"Erro a recuperar a imagem: {e}")
        return {"ImageBase64": ""}
    
