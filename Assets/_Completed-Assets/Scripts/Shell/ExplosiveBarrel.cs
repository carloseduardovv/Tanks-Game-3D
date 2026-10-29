using UnityEngine;
using System.Collections.Generic;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Efectos")]
    public GameObject explosionPrefab; // Asigna "ShellExplosion" o "TankExplosion" desde Prefabs

    [Header("Ajustes de Explosión")]
    public float explosionRadius = 10f;
    public float damageAmount = 50f; // 50% de daño (de 100 HP)
    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        string nameLower = other.gameObject.name.ToLower();
        if (nameLower.Contains("ground") || nameLower.Contains("plane") || nameLower.Contains("terrain"))
            return;

        Explode();
    }

    void Explode()
    {
        hasExploded = true;

        // 1. Instanciar la animación y audio de explosión
        if (explosionPrefab != null)
        {
            GameObject fxInstance = Instantiate(explosionPrefab, transform.position, transform.rotation);

            // Activar partículas si están detenidas
            ParticleSystem particles = fxInstance.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }

            // Reproducir sonido si tiene AudioSource
            AudioSource audio = fxInstance.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }

            // Destruir el objeto de efectos tras finalizar (3 segundos)
            Destroy(fxInstance, 3f);
        }

        // 2. Aplicar el 50% de daño a los tanques cercanos
        HashSet<GameObject> damagedTanks = new HashSet<GameObject>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            Transform tankRoot = col.transform.root;

            if (tankRoot.name.Contains("Tank") && !damagedTanks.Contains(tankRoot.gameObject))
            {
                damagedTanks.Add(tankRoot.gameObject);
                tankRoot.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
            }
        }

        // 3. Destruir el barril de la escena
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}